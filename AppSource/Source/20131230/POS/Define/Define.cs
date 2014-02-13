using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using CPC.POS.ViewModel;
using System;
using System.Net.Mail;
using SecurityLib;
using System.Net;

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

        /// <summary>
        /// Get Delay when user input in textbox search
        /// </summary>
        public static int DelaySearching = CPC.POS.Properties.Settings.Default.DelaySearching;

        public static string ApplicationFolder = Application.ResourceAssembly.Location.Remove(Application.ResourceAssembly.Location.Length - Application.ResourceAssembly.Location.Split('\\').Last().Count());

        public static string UpdateFileName = ApplicationFolder + "UpdateProgram.exe";

        // Store web address of contact page
        public static string ContactUsURL = CPC.POS.Properties.Settings.Default.ContactUsURL;

        //Default Customer ID for GUID & GuestNo
        public static int DefaultGuestId = 1;

        // Store permissions of users
        public static UserPermissions UserPermissions;

        #region Report

        // Email format
        public const string EMAIL_FORMAT = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";
        // User for change group command
        public static bool IS_CHANGING_GROUP = false;

        // User is administrator
        public static bool IS_ADMIN = false;

        // Is user logout 
        public static bool IS_LOG_OUT = false;

        // Permission print copy
        public static bool SET_PRINT_COPY = false;

        // Permission View set copy report
        public static bool VIEW_SET_COPY = false;

        // Permission edit set copy report
        public static bool NEW_SET_COPY = false;

        // Permission delete set copy report
        public static bool DELETE_SET_COPY = false;

        // Store previous screen 
        public static int PREVIOUS_SCREEN = 2;

        // Permission add new report
        public static bool ADD_REPORT = false;

        // Permission edit report
        public static bool EDIT_REPORT = false;

        // Permission delete report
        public static bool DELETE_REPORT = false;

        // Permission no show report
        public static bool NO_SHOW_REPORT = false;

        // Permission change group report
        public static bool CHANGE_GROUP_REPORT = false;

        // Current language
        public static string CURRENT_LANGUAGE = "EN";

        // Decimal places
        public static short DECIMAL_PLACES = 2;

        // Report currency name
        public static string RPT_CURRENCY_SYMBOL = "CurrencySymbol";

        // Permission privew report
        public static bool PREVIEW_REPORT = false;

        // Show print button in        
        public static bool SHOW_PRINT_BUTTON = false;

        // Permission print report
        public static bool PRINT_REPORT = false;

        // Permission set authorizse report
        public static bool SET_ASSIGN_AUTHORIZE_REPORT = false;

        // Permission set permission
        public static bool SET_PERMISSION = false;

        #region -DataTable list-
        public const string DT_HEADER = "Header";
        public const string DT_PRODUCT = "Product";
        public const string DT_PRODUCT_SUB = ""; /////////////////////
        public const string DT_SALE_ORDER = "SaleOrder";
        public const string DT_QTY_COST_ADJUSTMENT = "QuantityAdjustment";
        public const string DT_TRANSFER_HISTORY = "TransferHistory";
        public const string DT_DEPARTMENT = "Department";
        public const string DT_REORDER_STOCK = "ReOrderStock";
        public const string DT_PRODUCT_LIST = "ProductList";
        public const string DT_PRODUCT_ACTIVITY = ""; ///////////
        public const string DT_PRODUCT_SUMMARY_ACTIVITY = "ProductSumaryActivity";
        public const string DT_SALE_BY_PRODUCT_SUMMARY = "SaleByProductSummary";
        public const string DT_TRANSFER_DETAILS = "TranferHistoryDetails";
        public const string DT_SALE_BY_PRODUCT_DETAILS = "SaleByProductDetails";
        public const string DT_SALE_PROFIT_SUMMARY = "SaleProfitSummary";
        public const string DT_SALE_ORDER_OPERATION = "SaleOrderOperation";
        public const string DT_CUSTOMER_PAYMENT_SUMMARY = "CustomerPaymentSummary";
        public const string DT_CUSTOMER_PAYMENT_DETAILS = "CustomerPaymentDetails";
        public const string DT_CUSTOMER_ORDER_HISTORY = "CustomerOrderHistory";
        public const string DT_SALE_REPRESENTATIVE = "SaleRepresentative";
        public const string DT_PRODUCT_CUSTOMER = "ProductCustomer";
        public const string DT_PO_SUMMARY = "POSummary";
        public const string DT_PO_DETAILS = "PODetails";
        public const string DT_PRODUCT_COST = "ProductCost";
        public const string DT_VENDOR_PRODUCT_LIST = "VendorProductList";
        public const string DT_VENDOR_LIST = "VendorList";
        public const string DT_SALE_COMMISSION = "SaleRepresentativeCommission";
        public const string DT_SALE_COMMISSION_DETAILS = "SaleCommissionDetails";
        public const string DT_GIFT_CERTIFICATE = "GiftCertificateList";
        public const string DT_VOIDED_INVOICE = "VoidedInvoice";
        public const string DT_SOPO_LOCKED = "SOPOLocked";
        #endregion

        #region -Report name-
        // Inventory Report
        public const string RPT_PRODUCT_LIST = "rptProductList";
        public const string RPT_COST_ADJUSTMENT = "rptCostAdjustment";
        public const string RPT_QTY_ADJUSTMENT = "rptQuantityAdjustment";
        public const string RPT_PRODUCT_SUMMARY_ACTIVITY = "rptProductSummaryActivity";
        public const string RPT_CATEGORY_LIST = "rptCategoryList";
        public const string RPT_REORDER_STOCK = "rptReOrderStock";
        public const string RPT_TRANSFER_HISTORY = "rptTransferHistory";
        public const string RPT_TRANSER_DETAILS = "rptTransferHistoryDetails";
        // Purchasing Report
        public const string RPT_PO_SUMMARY = "rptPOSummary";
        public const string RPT_PO_DETAILS = "rptPODetails";
        public const string RPT_PRODUCT_COST = "rptProductCost";
        public const string RPT_VENDOR_PRODUCT_LIST = "rptVendorProductList";
        public const string RPT_VENDOR_LIST = "rptVendorList";
        public const string RPT_PO_LOCKED = "rptPOLocked";
        // Sale Report
        public const string RPT_SALE_BY_PRODUCT_SUMMARY = "rptSaleByProductSummary";
        public const string RPT_SALE_BY_PRODUCT_DETAILS = "rptSaleByProductDetails";
        public const string RPT_SALE_ORDER_SUMMARY = "rptSaleOrderSummary";
        public const string RPT_SALE_PROFIT_SUMMARY = "rptSaleProfitSummary";
        public const string RPT_SALE_ORDER_OPERATION = "rptSaleOrderOperational";
        public const string RPT_CUSTOMER_PAYMENT_SUMMARY = "rptCustomerPaymentSummary";
        public const string RPT_CUSTOMER_PAYMENT_DETAILS = "rptCustomerPaymentDetails";
        public const string RPT_PRODUCT_CUSTOMER = "rptProductCustomer";
        public const string RPT_CUSTOMER_ORDER_HISTORY = "rptCustomerOrderHistory";
        public const string RPT_SALE_REPRESENTATIVE = "rptSaleRepresentative";
        public const string RPT_SALE_COMMISSION = "rptSaleCommission";
        public const string RPT_SALE_COMMISSION_DETAILS = "rptSaleCommissionDetails";
        public const string RPT_GIFT_CERTIFICATE_LIST = "rptGiftCertificateList";
        public const string RPT_VOIDED_INVOICE = "rptVoidedInvoice";
        public const string RPT_SO_LOCKED = "rptSOLocked";
        #endregion

        #region -Public method-

        #region -Format date & Time-
        /// <summary>
        /// Format date
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>formated date</returns>
        public static string ToShortDateString(object dt)
        {
            return (dt == DBNull.Value) ? "" : string.Format(DateFormat, dt);
        }
        /// <summary>
        /// Format time
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToShortTimeString(DateTime dt)
        {
            return (dt == null) ? "" : dt.ToLongTimeString();
        }
        #endregion

        #region -Phone number format-
        /// <summary>
        /// Format phone nubmer like (###) ###-####
        /// </summary>
        /// <param name="phoneNumber">Phone to format</param>
        /// <returns></returns>
        public static string PhoneNumberFormat(string phoneNumber)
        {
            if (phoneNumber.Length == 10)
            {
                return string.Format("({0}) {1}-{2}",
                                            phoneNumber.Substring(0, 3),
                                            phoneNumber.Substring(3, 3),
                                            phoneNumber.Substring(6)
                                        );
            }
            return phoneNumber;
        }
        #endregion

        #region -Send email-

        // POP3 server 
        public static string POP3_EMAIL_SERVER = string.Empty;
        // Port to send email
        public static int POP3_PORT_SERVER = 0;
        // Email account to send mail
        public static string EMAIL_ACCOUNT = string.Empty;
        // Password for email account
        public static string EMAIL_PWD = string.Empty;
        // Number of day keep history printed list
        public static short KEEP_LOG = 31;

        /// <summary>
        /// Send email with attach file
        /// </summary>
        /// <param name="mailTo">email receiver</param>
        /// <param name="reportName">report name</param>
        /// <param name="attachmentFile">report pdf pdf file</param>
        /// <returns>error list</returns>
        public static string SendEmail(string mailTo, string reportName, string attachmentFile)
        {
            string errorList = string.Empty;
            string body = string.Empty;
            // Decrypt password
            string pwd = AESSecurity.Decrypt(EMAIL_PWD);
            SmtpClient smtpClient = new SmtpClient(POP3_EMAIL_SERVER, POP3_PORT_SERVER);
            smtpClient.Credentials = new NetworkCredential(EMAIL_ACCOUNT, pwd);
            // Read Mail templete from text file
            using (StreamReader reader = new StreamReader(EmailContentSendReportFile))
            {
                body = reader.ReadToEnd();
            }
            try
            {
                MailMessage mailMessage;
                mailMessage = new MailMessage();
                // Attach file
                Attachment att = new Attachment(attachmentFile);
                // Add attachment file
                mailMessage.Attachments.Add(att);
                // Add subject for email
                mailMessage.Subject = "[Auto delivery]-" + reportName;
                mailMessage.From = new MailAddress(EMAIL_ACCOUNT, "Smart POS");
                string[] mails = mailTo.Split(';');
                int sent = 0;
                foreach (string mail in mails)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(mail, EMAIL_FORMAT, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        // Add receiver
                        mailMessage.To.Add(mail);
                        // Set body email 
                        mailMessage.Body = body;
                        // Send email 
                        smtpClient.Send(mailMessage);
                        mailMessage.To.Clear();
                        sent++;
                        if (sent > 0 && sent % 5 == 0)
                        {
                            // Sleep 2 minutes after sent 5 email.
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                    else
                    {
                        errorList = mail + "\n";
                    }
                }
                // Send email                
                return errorList;
            }
            catch (UnauthorizedAccessException)
            {
                return errorList;
            }
        }

        #endregion

        #endregion

        #endregion

        //Define size of image in Image Control
        public static Size ImageSize = new Size { Width = 120, Height = 120 };
    }
}