using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using CPC.POS.ViewModel;
using System.Windows;
using System.Reflection;
using System.Linq;

namespace CPC.POS
{
    public sealed class Define
    {
        static Define()
        {
            DirectoryInfo directoryExecuting = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            DirectoryInfo mailTemplate = directoryExecuting.GetDirectories(@"Language\MailTemplate").FirstOrDefault();
            if (mailTemplate != null)
            {
                FileInfo templateFileInfo = mailTemplate.GetFiles("HappyBirthday-en-US.txt").FirstOrDefault();
                if (templateFileInfo != null)
                {
                    Define.ContentHappyBirthdayFile = templateFileInfo.FullName;
                }

                templateFileInfo = mailTemplate.GetFiles("RewardContentTemplate.txt").FirstOrDefault();
                if (templateFileInfo != null)
                {
                    Define.RewardContentTemplateFile = templateFileInfo.FullName;
                }

                templateFileInfo = mailTemplate.GetFiles(@"MailContentSendReportFile.txt").FirstOrDefault();
                if (templateFileInfo != null)
                {
                    Define.EmailContentSendReportFile = templateFileInfo.FullName;
                }
            }
        }
        //To define userpostgres to execute data in sql.
        public static string UserPostgres = "postgres";
        // Define the messages for Messenger: Access to language XML's Messenger node.
        public static string USER_LOGOUT_RESULT = CPC.POS.Properties.Settings.Default.USER_LOGOUT_RESULT;
        public static string USER_LOGIN_RESULT = CPC.POS.Properties.Settings.Default.USER_LOGIN_RESULT;

        public static string REMEMBER_KEY = CPC.POS.Properties.Settings.Default.Remember;
        public static string ADMIN_ACCOUNT = CPC.POS.Properties.Settings.Default.ADMIN_ACCOUNT;
        public static string ADMIN_PASSWORD = CPC.POS.Properties.Settings.Default.ADMIN_PASSWORD;

        /// <summary>
        /// Temporary password.
        /// </summary>
        public static string PasswordTemp = CPC.POS.Properties.Settings.Default.PasswordTemp;

        /// <summary>
        /// Default payment method is Cash
        /// </summary>
        public const int DefaultCashPayment = 1;
        public static string DefaultPassword = "!1Username";
        public static string CurrencyFormat;
        public static string CurrencySymbol;
        public static string NumericFormat;
        public static int DecimalPlaces = 0;
        public static int NumericDecimalDigits = 2;
        public static CultureInfo ConverterCulture;
        public static TextAlignment TextNumberAlign;
        public static string StateDisplayMemberPath = "Text";
        // Define the program name for singleton
        public static string programName = CPC.POS.Properties.Settings.Default.ProjectName;

        // Define GuestNo format
        public static string GuestNoFormat = CPC.POS.Properties.Settings.Default.GuestNoFormat;

        public static string PurchaseOrderNoFormat = CPC.POS.Properties.Settings.Default.PurchaseOrderNoFormat;

        public static string SaleOrderNoFormat = CPC.POS.Properties.Settings.Default.SaleOrderNoFormat;

        // Define number of display items on DataGrid
        public static int NumberOfDisplayItems = CPC.POS.Properties.Settings.Default.NumberOfDisplayItems;

        // Allow display loading box
        public static bool DisplayLoading = ConfigurationManager.AppSettings["DisplayLoading"] == "1";

        // Define default color note
        public static string DefaultColorNote = CPC.POS.Properties.Settings.Default.DefaultColorNote;

        // Define max number of image on ImageControl.
        public static int MaxNumberOfImages = CPC.POS.Properties.Settings.Default.MaxNumberOfImages;

        // Define MultiTax max length
        public static int MultiTaxMaxLength = CPC.POS.Properties.Settings.Default.MultiTaxMaxLength;

        // Define ProductCode format
        public static string ProductCodeFormat = CPC.POS.Properties.Settings.Default.ProductCodeFormat;

        //Default StoreCode
        public static int StoreCode = -1;

        //Default ShiftCode 
        public static string ShiftCode = null;

        //Default ShiftCode 
        public static string Username = CPC.POS.Properties.Settings.Default.Username;

        //Default ShiftCode 
        public static string Password = CPC.POS.Properties.Settings.Default.Password;

        //Max lenght display
        public static int MaxSerialLenght = CPC.POS.Properties.Settings.Default.MaxSerialLenght;

        public static int NumberOfSerialDisplay = CPC.POS.Properties.Settings.Default.NumberOfSerialDisplay;

        public static string IntegerFormat = CPC.POS.Properties.Settings.Default.IntegerFormat;

        public static string DecimalFormat = CPC.POS.Properties.Settings.Default.DecimalFormat;

        //Define Configuration
        /// <summary>
        /// Property get configuration table from database
        ///<para>Need check null after use</para>
        /// </summary>
        public static CPC.POS.Model.base_ConfigurationModel CONFIGURATION;

        //To get information of user.
        public static CPC.POS.Model.base_ResourceAccountModel USER;

        //To get authorization of user.
        public static ObservableCollection<CPC.POS.Model.base_AuthorizeModel> USER_AUTHORIZATION
        {
            get;
            set;
        }

        public static SynchronizationViewModel SynchronizationViewModel;

        // Format.
        public static string DateFormat = ConfigurationManager.AppSettings["DateFormat"];

        // Format date for sticky
        public static string StampDateFormat = ConfigurationManager.AppSettings["StampDateFormat"];

        public static string ContentHappyBirthdayFile = null;
        public static string RewardContentTemplateFile = null;
        public static string EmailContentSendReportFile = string.Empty;
    }
}
