using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;
using System.Security.AccessControl;
using IWshRuntimeLibrary;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration.Install;
using System.Collections;

public class InstallHelper
{
    const string UninstallCurrentUser = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    const string UninstallLocalMachine32 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    const string UninstallLocalMachine64 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

    #region Folder Properties

    public static string ProgramsFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.Programs); }
    }

    public static string ProgramFilesFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles); }
    }

    public static string ApplicationDataFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); }
    }

    public static string CommonApplicationDataFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); }
    }

    public static string CommonProgramFilesFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles); }
    }

    public static string CookiesFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.Cookies); }
    }

    public static string DesktopDirectoryFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); }
    }

    public static string FavoritesFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.Favorites); }
    }

    public static string HistoryFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.History); }
    }

    public static string InternetCacheFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.InternetCache); }
    }

    public static string LocalApplicationDataFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }
    }

    public static string MyComputerFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.MyComputer); }
    }

    public static string MyDocumentsFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); }
    }

    public static string MyMusicFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic); }
    }

    public static string MyPicturesFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); }
    }

    public static string PersonalFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); }
    }

    public static string RecentFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.Recent); }
    }

    public static string SendToFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.SendTo); }
    }

    public static string TemplatesFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.Templates); }
    }

    public static string StartupFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.Startup); }
    }

    public static string SystemFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.System); }
    }

    public static string StartMenuFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.StartMenu); }
    }

    public static string DesktopFolder
    {
        get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); }
    }

    public static string AssemblyFolder
    {
        get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
    }

    public static string QuickLaunchFolder
    {
        get
        {
            string path1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path2 = @"\Microsoft\Internet Explorer\Quick Launch";
            return String.Concat(path1, path2);
        }
    }

    #endregion

    #region Methods

    #region Methods Create Shortcut

    public static void CreateShortcut(string shortcutPath, string iconPath, string targetPath)
    {
        CreateShortcut(shortcutPath, iconPath, targetPath, null);
    }

    public static void CreateShortcut(string shortcutPath, string iconPath, string targetPath, string hotKey)
    {
        CreateShortcut(shortcutPath, iconPath, targetPath, hotKey, null);
    }

    public static void CreateShortcut(string shortcutPath, string iconPath, string targetPath, string hotKey, string description)
    {
        CreateShortcut(shortcutPath, iconPath, targetPath, hotKey, description, null);
    }

    public static void CreateShortcut(string shortcutPath, string iconPath, string targetPath, string hotKey, string description, string arguments)
    {
        CreateShortcut(shortcutPath, iconPath, targetPath, hotKey, description, arguments, null);
    }

    public static void CreateShortcut(string shortcutPath, string iconPath, string targetPath, string hotKey, string description, string arguments, string workingDirectory)
    {
        CreateShortcut(shortcutPath, iconPath, targetPath, hotKey, description, arguments, workingDirectory, -1);
    }

    public static void CreateShortcut(string shortcutPath, string iconPath, string targetPath, string hotKey, string description, string arguments, string workingDirectory, int windowStyle)
    {
        try
        {
            string parentFolder = Path.GetDirectoryName(shortcutPath);
            if (!string.IsNullOrEmpty(parentFolder) && !Directory.Exists(parentFolder))
            {
                Directory.CreateDirectory(parentFolder);
            }

            WshShellClass wshShellClass = new WshShellClass();
            IWshShortcut shortcut = (IWshShortcut)wshShellClass.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.IconLocation = iconPath;
            if (hotKey != null) shortcut.Hotkey = hotKey;
            if (description != null) shortcut.Description = description;
            if (arguments != null) shortcut.Arguments = arguments;
            if (workingDirectory != null) shortcut.WorkingDirectory = workingDirectory;
            if (windowStyle != null) shortcut.WindowStyle = windowStyle;
            shortcut.Save();
        }
        catch
        {
            throw;
        }
    }

    #endregion

    //#region DeleteFile

    //public static void DeleteFile(string filePath)
    //{
    //    try
    //    {
    //        if (System.IO.File.Exists(filePath))
    //        {
    //            System.IO.File.Delete(filePath);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void DeleteFiles(string directoryPath)
    //{
    //    try
    //    {
    //        foreach (string file in Directory.GetFiles(directoryPath))
    //        {
    //            System.IO.File.Delete(file);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void DeleteFiles(string directoryPath, string searchPattern)
    //{
    //    try
    //    {
    //        foreach (string file in Directory.GetFiles(directoryPath, searchPattern))
    //        {
    //            System.IO.File.Delete(file);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void DeleteFiles(string directoryPath, string searchPattern, SearchOption searchOption)
    //{
    //    try
    //    {
    //        foreach (string file in Directory.GetFiles(directoryPath, searchPattern, searchOption))
    //        {
    //            System.IO.File.Delete(file);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //#endregion UpdateAppSettings

    //#region CopyFile

    //public static void CopyFile(string sourceFileName, string destDirectory)
    //{
    //    try
    //    {
    //        FileInfo fileInfo = new FileInfo(sourceFileName);
    //        string destFileName = destDirectory + "\\" + fileInfo.Name;
    //        System.IO.File.Copy(sourceFileName, destFileName);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void CopyFile(string sourceFileName, string destDirectory, bool overwrite)
    //{
    //    try
    //    {
    //        FileInfo fileInfo = new FileInfo(sourceFileName);
    //        string destFileName = destDirectory + "\\" + fileInfo.Name;
    //        System.IO.File.Copy(sourceFileName, destFileName, overwrite);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void CopyFiles(string sourceDirectory, string destDirectory)
    //{
    //    try
    //    {
    //        foreach (string fileName in Directory.GetFiles(sourceDirectory))
    //        {
    //            CopyFile(fileName, destDirectory);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void CopyFiles(string sourceDirectory, string destDirectory, bool overwrite)
    //{
    //    try
    //    {
    //        foreach (string fileName in Directory.GetFiles(sourceDirectory))
    //        {
    //            CopyFile(fileName, destDirectory, overwrite);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void CopyFiles(string sourceDirectory, string searchPattern, string destDirectory)
    //{
    //    try
    //    {
    //        foreach (string fileName in Directory.GetFiles(sourceDirectory, searchPattern))
    //        {
    //            CopyFile(fileName, destDirectory);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void CopyFiles(string sourceDirectory, string searchPattern, string destDirectory, bool overwrite)
    //{
    //    try
    //    {
    //        foreach (string fileName in Directory.GetFiles(sourceDirectory, searchPattern))
    //        {
    //            CopyFile(fileName, destDirectory, overwrite);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void CopyFiles(string sourceDirectory, string searchPattern, SearchOption searchOption, string destDirectory)
    //{
    //    try
    //    {
    //        foreach (string fileName in Directory.GetFiles(sourceDirectory, searchPattern, searchOption))
    //        {
    //            CopyFile(fileName, destDirectory);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void CopyFiles(string sourceDirectory, string searchPattern, SearchOption searchOption, string destDirectory, bool overwrite)
    //{
    //    try
    //    {
    //        foreach (string fileName in Directory.GetFiles(sourceDirectory, searchPattern, searchOption))
    //        {
    //            CopyFile(fileName, destDirectory, overwrite);
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //#endregion

    //#region DeleteDirectory

    //public static void DeleteDirectory(string directoryPath)
    //{
    //    try
    //    {
    //        Directory.Delete(directoryPath);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void DeleteDirectory(string directoryPath, bool recursive)
    //{
    //    try
    //    {
    //        Directory.Delete(directoryPath, recursive);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //#endregion

    //#region CreateDirectory

    //public static void CreateDirectory(string path)
    //{
    //    try
    //    {
    //        Directory.CreateDirectory(path);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //#endregion

    //#region UpdateAppSettings

    //public static void UpdateAppSettings(string xmlFilePath, string key, string value)
    //{
    //    try
    //    {
    //        XmlDocument xmlDocument = new XmlDocument();
    //        xmlDocument.Load(xmlFilePath);

    //        XmlElement appSettingsElement = xmlDocument.DocumentElement["appSettings"];
    //        if (appSettingsElement == null)
    //        {
    //            throw new NullReferenceException("AppSettings element not found.");
    //        }

    //        foreach (XmlNode xmlNode in appSettingsElement.ChildNodes)
    //        {
    //            if (xmlNode.Attributes["key"].Value.Equals(key))
    //            {
    //                xmlNode.Attributes["value"].Value = value;
    //                break;
    //            }
    //        }
    //        xmlDocument.Save(xmlFilePath);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //#endregion

    //#region SelectNode

    //public static XmlNode SelectSingleNode(string xmlFilePath, string xPath)
    //{
    //    XmlNode xmlNode = null;

    //    try
    //    {
    //        XmlDocument xmlDocument = new XmlDocument();
    //        xmlDocument.Load(xmlFilePath);
    //        xmlNode = xmlDocument.SelectSingleNode(xPath);
    //    }
    //    catch
    //    {
    //        throw;
    //    }

    //    return xmlNode;
    //}

    //public static XmlNodeList SelectNodes(string xmlFilePath, string xPath)
    //{
    //    XmlNodeList xmlNodeList = null;

    //    try
    //    {
    //        XmlDocument xmlDocument = new XmlDocument();
    //        xmlDocument.Load(xmlFilePath);
    //        xmlNodeList = xmlDocument.SelectNodes(xPath);
    //    }
    //    catch
    //    {
    //        throw;
    //    }

    //    return xmlNodeList;
    //}

    //#endregion

    #region CreateDatabaseOnPostgreSQL

    public static void CreateDatabaseOnPostgreSQL(string queryFilePath, string hostPostgre, string userName, string password, string databaseName)
    {
        try
        {
            string postgreInstallLocation = GetInstallLocation("PostgreSQL");
            if (string.IsNullOrEmpty(postgreInstallLocation))
            {
                throw new Exception("PostgreSQL install location not found.");
            }

            string postgreBinPath = postgreInstallLocation.Substring(2) + "\\Bin";
            string postgreInstallPathRoot = postgreInstallLocation.Substring(0, 2);
            queryFilePath = queryFilePath.Replace('\\', '/');

            string batchFile = AssemblyFolder + @"\CreateDatabaseBatchFile.bat";
            StreamWriter streamWriter = System.IO.File.CreateText(batchFile);
            streamWriter.WriteLine("cls");
            streamWriter.WriteLine("echo On");
            streamWriter.WriteLine(string.Format("set PGHOST={0}", hostPostgre));
            streamWriter.WriteLine(string.Format("set PGUSERNAME={0}", userName));
            streamWriter.WriteLine(string.Format("set PGPASSWORD={0}", password));
            streamWriter.WriteLine("cd \\");
            streamWriter.WriteLine(postgreInstallPathRoot);
            streamWriter.WriteLine(string.Format(@"cd {0}", postgreBinPath));
            streamWriter.WriteLine(string.Format("dropdb -U %PGUSERNAME% -e {0}", databaseName));
            streamWriter.WriteLine(string.Format("createdb -U %PGUSERNAME% -e {0}", databaseName));
            streamWriter.WriteLine(string.Format("psql -U %PGUSERNAME% -d {0} -c\"\\i '{1}'\"", databaseName, queryFilePath));
            streamWriter.WriteLine("pause");
            streamWriter.WriteLine("exit");
            streamWriter.Close();

            ExecuteFile(batchFile);
        }
        catch
        {
            throw;
        }
    }

    #endregion

    #region ExecuteFile

    public static void ExecuteFile(string filePath)
    {
        try
        {
            Process p = Process.Start(filePath);
            //BringWindowToTop(p.MainWindowHandle); // Make sure the user will see the new window above of the setup.
            p.WaitForExit();
        }
        catch
        {
            throw;
        }
    }

    #endregion

    //#region ExistApplicationInstalled

    //public static bool ExistApplicationInstalled(string displayName)
    //{
    //    RegistryKey registryKey;
    //    string displayNameRegistryKey;
    //    string name = "DisplayName";

    //    registryKey = Registry.CurrentUser.OpenSubKey(UninstallCurrentUser);
    //    if (registryKey != null)
    //    {
    //        RegistryKey subRegistryKey;
    //        foreach (string keyName in registryKey.GetSubKeyNames())
    //        {
    //            subRegistryKey = registryKey.OpenSubKey(keyName);
    //            displayNameRegistryKey = subRegistryKey.GetValue(name) as string;
    //            if (displayNameRegistryKey != null)
    //            {
    //                displayNameRegistryKey = displayNameRegistryKey.Trim();
    //                if (displayNameRegistryKey.Contains(displayName))
    //                {
    //                    return true;
    //                }
    //            }
    //        }
    //    }

    //    registryKey = Registry.LocalMachine.OpenSubKey(UninstallLocalMachine32);
    //    if (registryKey != null)
    //    {
    //        RegistryKey subRegistryKey;
    //        foreach (string keyName in registryKey.GetSubKeyNames())
    //        {
    //            subRegistryKey = registryKey.OpenSubKey(keyName);
    //            displayNameRegistryKey = subRegistryKey.GetValue(name) as string;
    //            if (displayNameRegistryKey != null)
    //            {
    //                displayNameRegistryKey = displayNameRegistryKey.Trim();
    //                if (displayNameRegistryKey.Contains(displayName))
    //                {
    //                    return true;
    //                }
    //            }
    //        }
    //    }

    //    registryKey = Registry.LocalMachine.OpenSubKey(UninstallLocalMachine64);
    //    if (registryKey != null)
    //    {
    //        RegistryKey subRegistryKey;
    //        foreach (string keyName in registryKey.GetSubKeyNames())
    //        {
    //            subRegistryKey = registryKey.OpenSubKey(keyName);
    //            displayNameRegistryKey = subRegistryKey.GetValue(name) as string;
    //            if (displayNameRegistryKey != null)
    //            {
    //                displayNameRegistryKey = displayNameRegistryKey.Trim();
    //                if (displayNameRegistryKey.Contains(displayName))
    //                {
    //                    return true;
    //                }
    //            }
    //        }
    //    }

    //    return false;
    //}

    //#endregion

    //#region GetDisplayVersion

    //public static Version GetDisplayVersion(string displayName)
    //{
    //    RegistryKey registryKey;
    //    string displayNameRegistryKey;
    //    string dName = "DisplayName";
    //    string dVersion = "DisplayVersion";

    //    registryKey = Registry.CurrentUser.OpenSubKey(UninstallCurrentUser);
    //    if (registryKey != null)
    //    {
    //        RegistryKey subRegistryKey;
    //        foreach (string keyName in registryKey.GetSubKeyNames())
    //        {
    //            subRegistryKey = registryKey.OpenSubKey(keyName);
    //            displayNameRegistryKey = subRegistryKey.GetValue(dName) as string;
    //            if (displayNameRegistryKey != null)
    //            {
    //                displayNameRegistryKey = displayNameRegistryKey.Trim();
    //                if (displayNameRegistryKey.Contains(displayName))
    //                {
    //                    if (subRegistryKey.GetValue(dVersion) != null)
    //                    {
    //                        return new Version(subRegistryKey.GetValue(dVersion) as string);
    //                    }
    //                    else
    //                    {
    //                        return null;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    registryKey = Registry.LocalMachine.OpenSubKey(UninstallLocalMachine32);
    //    if (registryKey != null)
    //    {
    //        RegistryKey subRegistryKey;
    //        foreach (string keyName in registryKey.GetSubKeyNames())
    //        {
    //            subRegistryKey = registryKey.OpenSubKey(keyName);
    //            displayNameRegistryKey = subRegistryKey.GetValue(dName) as string;
    //            if (displayNameRegistryKey != null)
    //            {
    //                displayNameRegistryKey = displayNameRegistryKey.Trim();
    //                if (displayNameRegistryKey.Contains(displayName))
    //                {
    //                    if (subRegistryKey.GetValue(dVersion) != null)
    //                    {
    //                        return new Version(subRegistryKey.GetValue(dVersion) as string);
    //                    }
    //                    else
    //                    {
    //                        return null;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    registryKey = Registry.LocalMachine.OpenSubKey(UninstallLocalMachine64);
    //    if (registryKey != null)
    //    {
    //        RegistryKey subRegistryKey;
    //        foreach (string keyName in registryKey.GetSubKeyNames())
    //        {
    //            subRegistryKey = registryKey.OpenSubKey(keyName);
    //            displayNameRegistryKey = subRegistryKey.GetValue(dName) as string;
    //            if (displayNameRegistryKey != null)
    //            {
    //                displayNameRegistryKey = displayNameRegistryKey.Trim();
    //                if (displayNameRegistryKey.Contains(displayName))
    //                {
    //                    if (subRegistryKey.GetValue(dVersion) != null)
    //                    {
    //                        return new Version(subRegistryKey.GetValue(dVersion) as string);
    //                    }
    //                    else
    //                    {
    //                        return null;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    return null;
    //}

    //#endregion

    //#region GetInstallLocation

    public static string GetInstallLocation(string displayName)
    {
        RegistryKey registryKey;
        string displayNameRegistryKey;
        string dName = "DisplayName";
        string installLocation = "InstallLocation";

        registryKey = Registry.CurrentUser.OpenSubKey(UninstallCurrentUser);
        if (registryKey != null)
        {
            RegistryKey subRegistryKey;
            foreach (string keyName in registryKey.GetSubKeyNames())
            {
                subRegistryKey = registryKey.OpenSubKey(keyName);
                displayNameRegistryKey = subRegistryKey.GetValue(dName) as string;
                if (displayNameRegistryKey != null)
                {
                    displayNameRegistryKey = displayNameRegistryKey.Trim();
                    if (displayNameRegistryKey.Contains(displayName))
                    {
                        return subRegistryKey.GetValue(installLocation) as string;
                    }
                }
            }
        }

        registryKey = Registry.LocalMachine.OpenSubKey(UninstallLocalMachine32);
        if (registryKey != null)
        {
            RegistryKey subRegistryKey;
            foreach (string keyName in registryKey.GetSubKeyNames())
            {
                subRegistryKey = registryKey.OpenSubKey(keyName);
                displayNameRegistryKey = subRegistryKey.GetValue(dName) as string;
                if (displayNameRegistryKey != null)
                {
                    displayNameRegistryKey = displayNameRegistryKey.Trim();
                    if (displayNameRegistryKey.Contains(displayName))
                    {
                        return subRegistryKey.GetValue(installLocation) as string;
                    }
                }
            }
        }

        registryKey = Registry.LocalMachine.OpenSubKey(UninstallLocalMachine64);
        if (registryKey != null)
        {
            RegistryKey subRegistryKey;
            foreach (string keyName in registryKey.GetSubKeyNames())
            {
                subRegistryKey = registryKey.OpenSubKey(keyName);
                displayNameRegistryKey = subRegistryKey.GetValue(dName) as string;
                if (displayNameRegistryKey != null)
                {
                    displayNameRegistryKey = displayNameRegistryKey.Trim();
                    if (displayNameRegistryKey.Contains(displayName))
                    {
                        return subRegistryKey.GetValue(installLocation) as string;
                    }
                }
            }
        }

        return null;
    }

    //#endregion

    #region Compare

    public static int Compare(Version version1, Version version2)
    {
        return version1.CompareTo(version2);
    }

    #endregion

    public static void OpenWithStartInfo(string program, InstallContext context)
    {
        string[] excludeKeys = new string[] { "Run", "WaitForExit" };

        ProcessStartInfo startInfo = new ProcessStartInfo(program);
        startInfo.WindowStyle = ProcessWindowStyle.Normal;
        startInfo.Arguments = ContextParametersToCommandArguments(context, excludeKeys);

        Trace.WriteLine("Run the program " + program + startInfo.Arguments);

        Process p = Process.Start(startInfo);
        //ShowWindow(p.MainWindowHandle, WindowShowStyle.Show); //otherwise it is not activated 
        //SetForegroundWindow(p.MainWindowHandle);
        BringWindowToTop(p.MainWindowHandle); // Make sure the user will see the new window above of the setup.

        Trace.WriteLine("The program Responding= " + p.Responding);

        if ((context.IsParameterTrue("WaitForExit")))
            p.WaitForExit(); // Have to hold the setup until the application is closed.
    }

    public static string ContextParametersToCommandArguments(InstallContext context, string[] excludeKeys)
    {
        excludeKeys = ToLower(excludeKeys);
        StringBuilder sb = new StringBuilder();
        foreach (DictionaryEntry de in context.Parameters)
        {
            string sKey = (string)de.Key;
            bool bAdd = true;
            if (excludeKeys != null)
            {
                bAdd = (Array.IndexOf(excludeKeys, sKey.ToLower()) < 0);
            }
            if (bAdd)
            {
                AppendArgument(sb, sKey, (string)de.Value);
            }
        }
        return sb.ToString();
    }

    public static StringBuilder AppendArgument(StringBuilder sb, String Key, string value)
    {
        sb.Append(" /");
        sb.Append(Key);
        //Note that if value is empty string, = sign is expected, e.g."/PORT="
        if (value != null)
        {
            sb.Append("=");
            sb.Append(value);
        }
        return sb;
    }

    #region  Library methods
    public static string[] ToLower(string[] strings)
    {
        if (strings != null)
        {
            for (int i = 0; i < strings.Length; i++)
                strings[i] = strings[i].ToLower();
        }
        return strings;
    }
    #endregion  //"FS library methods"

    #region  EnumShowWindow

    // http://pinvoke.net/default.aspx/user32.BringWindowToTop
    [DllImport("user32.dll")]
    static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    //from http://pinvoke.net/default.aspx/user32.SwitchToThisWindow 
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

    /// <summary>Enumeration of the different ways of showing a window using 
    /// ShowWindow</summary>
    private enum WindowShowStyle : uint
    {
        /// <summary>Hides the window and activates another window.</summary>
        /// <remarks>See SW_HIDE</remarks>
        Hide = 0,
        /// <summary>Activates and displays a window. If the window is minimized 
        /// or maximized, the system restores it to its original size and 
        /// position. An application should specify this flag when displaying 
        /// the window for the first time.</summary>
        /// <remarks>See SW_SHOWNORMAL</remarks>
        ShowNormal = 1,
        /// <summary>Activates the window and displays it as a minimized window.</summary>
        /// <remarks>See SW_SHOWMINIMIZED</remarks>
        ShowMinimized = 2,
        /// <summary>Activates the window and displays it as a maximized window.</summary>
        /// <remarks>See SW_SHOWMAXIMIZED</remarks>
        ShowMaximized = 3,
        /// <summary>Maximizes the specified window.</summary>
        /// <remarks>See SW_MAXIMIZE</remarks>
        Maximize = 3,
        /// <summary>Displays a window in its most recent size and position. 
        /// This value is similar to "ShowNormal", except the window is not 
        /// actived.</summary>
        /// <remarks>See SW_SHOWNOACTIVATE</remarks>
        ShowNormalNoActivate = 4,
        /// <summary>Activates the window and displays it in its current size 
        /// and position.</summary>
        /// <remarks>See SW_SHOW</remarks>
        Show = 5,
        /// <summary>Minimizes the specified window and activates the next 
        /// top-level window in the Z order.</summary>
        /// <remarks>See SW_MINIMIZE</remarks>
        Minimize = 6,
        /// <summary>Displays the window as a minimized window. This value is 
        /// similar to "ShowMinimized", except the window is not activated.</summary>
        /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
        ShowMinNoActivate = 7,
        /// <summary>Displays the window in its current size and position. This 
        /// value is similar to "Show", except the window is not activated.</summary>
        /// <remarks>See SW_SHOWNA</remarks>
        ShowNoActivate = 8,
        /// <summary>Activates and displays the window. If the window is 
        /// minimized or maximized, the system restores it to its original size 
        /// and position. An application should specify this flag when restoring 
        /// a minimized window.</summary>
        /// <remarks>See SW_RESTORE</remarks>
        Restore = 9,
        /// <summary>Sets the show state based on the SW_ value specified in the 
        /// STARTUPINFO structure passed to the CreateProcess function by the 
        /// program that started the application.</summary>
        /// <remarks>See SW_SHOWDEFAULT</remarks>
        ShowDefault = 10,
        /// <summary>Windows 2000/XP: Minimizes a window, even if the thread 
        /// that owns the window is hung. This flag should only be used when 
        /// minimizing windows from a different thread.</summary>
        /// <remarks>See SW_FORCEMINIMIZE</remarks>
        ForceMinimized = 11
    }
    #endregion

    #endregion
}
