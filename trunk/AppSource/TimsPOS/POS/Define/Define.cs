using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Globalization;

namespace CPC.POS
{
    public sealed class Define
    {
        // define the program name for singleton
        public static string programName = ConfigurationManager.AppSettings["ProjectName"];

        // define the program version
        public string version = ConfigurationManager.AppSettings["ProjectVersion"];

        // define the release date
        public string releaseDate = ConfigurationManager.AppSettings["ProjectReleaseDate"];

        // define the application folder using the program name
        public static string ApplicationFolderName = programName;

        // define the image folder
        public readonly static string ImageFilesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ApplicationFolderName) + @"\";

        // define the messages for Messenger: Access to language XML's Messenger node.
        public const string USER_LOGOUT_RESULT = "User logout result";
        public const string USER_LOGIN_RESULT = "User login authenicate result";
        public const string DATABASE_CONFIG_RESULT = "Not ready !!!";

        public const int None = 0;
        public const string REMEMBER_KEY = "REMEMBER";
        public const string ENTITY_CONN_KEY = "ENTITY_CONN_STRING";
        public const string RESOURCE_DATA = "Database.Tims";
        public const string ADMIN_ACCOUNT = "admin";
        public const string ADMIN_PASSWORD = "1234";

        /// <summary>
        /// ProductDeparment level is 0.
        /// </summary>
        public const short ProductDeparmentLevel = 0;
        /// <summary>
        /// ProductCategory level is 1
        /// </summary>
        public const short ProductCategoryLevel = 1;
        /// <summary>
        /// ProductBrand level is 2
        /// </summary>
        public const short ProductBrandLevel = 2;

        /// <summary>
        /// Temporary password.
        /// </summary>
        public const string PasswordTemp = "111111";

        /// <summary>
        /// Always payment method have 'default' property is 1.
        /// </summary>
        public const int AlwaysPaymentMethod = 1;

        // Define GuestNo format
        public static string GuestNoFormat = ConfigurationManager.AppSettings["GuestNoFormat"];

        public static string PurchaseOrderNoFormat = ConfigurationManager.AppSettings["PurchaseOrderNoFormat"];

        // Define number of display items on DataGrid
        public static string NumberOfDisplayItems = ConfigurationManager.AppSettings["GridTotalPage"];

        // Allow display loading box
        public static bool DisplayLoading = ConfigurationManager.AppSettings["DisplayLoading"] == "1";

        // Allow display grid navigator bar when TotalPage is greater than DisplayGridNavigatorBar
        public static int DisplayGridNavigatorBar = int.Parse(ConfigurationManager.AppSettings["DisplayGridNavigatorBar"]);

        // Define default color note
        public static string DefaultColorNote = ConfigurationManager.AppSettings["DefaultColorNote"];

        // Define max number of image on ImageControl.
        public static string MaxNumberOfImages = ConfigurationManager.AppSettings["MaxNumberOfImages"];

        // Define max number of note.
        public static int MaxNumberOfNotes = int.Parse(ConfigurationManager.AppSettings["MaxNumberOfNotes"]);
        public static int MaxTaxCodeOption = int.Parse(ConfigurationManager.AppSettings["MaxTaxCodeOption"]);

        //Define Configuration
        /// <summary>
        /// Property get configuration table from database
        ///<para>Need check null after use</para>
        /// </summary>
        public static CPC.POS.Model.base_ConfigurationModel CONFIGURATION;

        //To get information of user.
        public static CPC.POS.Model.base_ResourceAccountModel USER;

        //To get authorization of user.
        public static ObservableCollection<CPC.POS.Model.base_AuthorizeModel> USER_AUTHORIZATION { get; set; }

        // Define ProductCode format
        public static string ProductCodeFormat = ConfigurationManager.AppSettings["ProductCodeFormat"];

        //Default StoreCode 
        public static int StoreCode = 0;
   
        // Format.
        public static string DateFormat = ConfigurationManager.AppSettings["DateFormat"];
        public static string IntegerFormat = ConfigurationManager.AppSettings["IntegerFormat"];
        public static string DecimalFormat = ConfigurationManager.AppSettings["DecimalFormat"];
        public static string CurrencyFormat;
        public static string NumericFormat;
        public static CultureInfo ConverterCulture;
        public static string PercentFormat = ConfigurationManager.AppSettings["PercentFormat"];
        
        //Max lenght display
        public static int MaxSerialLenght = int.Parse(ConfigurationManager.AppSettings["MaxSerialLenght"]);

        public const byte NumberOfSerialDisplay = 5;
    }
}
