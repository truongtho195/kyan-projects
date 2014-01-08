using System;
using System.IO;
using CPC.TimeClock.Model;

namespace CPC.TimeClock
{
    /// <summary>
    /// Define all the global names 
    /// </summary>
    public sealed class define
    {
        // define the program name for singleton
        public const string programName = "Tims TimeClock";

        // define the program version
        public const string version = "1.0.0";

        // define the release date
        public const string releaseDate = "4/18/2012";

        // define the application folder using the program name
        public const string ApplicationFolderName = programName;

        // define the image folder
        public readonly static string ImageFilesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ApplicationFolderName) + @"\";

        // define the messages for Messenger
        public const string USER_LOGIN_RESULT = "User login authenicate result";
        public const string DATABASE_CONFIG_RESULT = "Database configuration authenicate result";

        public const string ENTITY_CONN_KEY = "ENTITY_CONN_STRING";
        public const string RESOURCE_DATA = "Database.Tims";

        public readonly static string IMG_EMPLOYEE_DIRECTORY = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ApplicationFolderName) + @"\";

        public static string DEFAULT_CONNECTIONSTRING
        {
            get { return System.Configuration.ConfigurationManager.ConnectionStrings["POSDBEntities"].ConnectionString; }
        }

        public static int MinimumBarCodeLength = 10;
        public static float hours = 8;
        public static bool BlockFingerprint = false;
        public static bool EnableIdleTime = true;
        public static bool blockRegisterIfNotCompleted = true;
        public static TimeSpan ReloadTime = TimeSpan.FromHours(12);
        public static TimeSpan IdleTime = TimeSpan.FromMinutes(1);
        public static base_ConfigurationModel CONFIGURATION;
    }
}