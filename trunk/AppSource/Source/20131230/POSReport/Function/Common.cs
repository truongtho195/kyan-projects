using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.POSReport.Properties;
using SecurityLib;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Reflection;

namespace CPC.POSReport.Function
{
    public static class Common
    {
        static Common()
        {
            // Get file path (file MailContentTemplate.txt)
            DirectoryInfo directoryExecuting = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            DirectoryInfo mailTemplate = directoryExecuting.GetDirectories(@"Language/Template").FirstOrDefault();
            if (mailTemplate != null)
            {
                FileInfo emailContentFileInfo = mailTemplate.GetFiles(@"MailContentTemplate.txt").FirstOrDefault();
                if (emailContentFileInfo != null)
                {
                    EmailContentSendReportFile = emailContentFileInfo.FullName;
                }
            }
        }
        
        // Constant
        // Date time format
        public const string DATE_FORMAT = "{0:MM/dd/yyyy}";
        // User name format
        public const string USERNAME_FORMAT = "[a-zA-Z0-9\\s]";
        // Login Name format
        public const string LOGIN_NAME_FORMAT = @"[\w^\f\n\r\t\v]{4,20}";
        // Passowrd format
        public const string PASSWORD_FORMAT = @"((?=.*[^a-zA-Z])(?=.*[a-z])(?=.*[A-Z])(?!\s).{8,20})";
        // Email format
        public const string EMAIL_FORMAT = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";
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
        // Email content file path
        public static string EmailContentSendReportFile = string.Empty;

        #region -Report name-
        // Inventory Report
        public const string RPT_PRODUCT_LIST = "rptProductList";
        public const string RPT_COST_ADJUSTMENT = "rptCostAdjustment";
        public const string RPT_QTY_ADJUSTMENT ="rptQuantityAdjustment";
        public const string RPT_PRODUCT_SUMMARY_ACTIVITY ="rptProductSummaryActivity";
        public const string RPT_CATEGORY_LIST ="rptCategoryList";
        public const string RPT_REORDER_STOCK ="rptReOrderStock";
        public const string RPT_TRANSFER_HISTORY ="rptTransferHistory";
        public const string RPT_TRANSER_DETAILS ="rptTransferHistoryDetails";
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

        // Default password in application
        public static string PWD_TEMP = "nauxGNAL@NDELhnyuq";
        // Current language
        public static string CURRENT_LANGUAGE = "EN";
        // Report currency name
        public static string RPT_CURRENCY_SYMBOL = "CurrencySymbol";
        // Report currency symbol
        public static string CURRENT_SYMBOL = "đ";
        // Decimal places
        public static short DECIMAL_PLACES = 2;
        // Administrator's name
        public static string ADMINISTRATOR_LOGIN_NAME  = Settings.Default.AdminUser;
        // Administrator's password
        public static string ADMIN_PWD = Settings.Default.AdminPwd;
        // Show print button in        
        public static bool SHOW_PRINT_BUTTON = false;
        // User is administrator
        public static bool IS_ADMIN = false;
        // Current user
        public static string LOGIN_NAME = string.Empty;
        // User resource for current user
        public static string USER_RESOURCE = string.Empty;
        // Store previous screen 
        public static int PREVIOUS_SCREEN = 2;
        // Is user logout 
        public static bool IS_LOG_OUT = false;

        // Permission print copy
        public static bool SET_PRINT_COPY = false;
        // Permission print report
        public static bool PRINT_REPORT = false;
        // Permission privew report
        public static bool PREVIEW_REPORT = false;
        // Permission add new report
        public static bool ADD_REPORT = false;
        // Permission edit report
        public static bool EDIT_REPORT = false;
        // Permission delete report
        public static bool DELETE_REPORT = false;
        // Permission change group report
        public static bool CHANGE_GROUP_REPORT = false;
        // Permission no show report
        public static bool NO_SHOW_REPORT = false;
        // Permission set authorizse report
        public static bool SET_ASSIGN_AUTHORIZE_REPORT = false;
        // Permission View set copy report
        public static bool VIEW_SET_COPY = false;
        // Permission edit set copy report
        public static bool NEW_SET_COPY = false;
        // Permission delete set copy report
        public static bool DELETE_SET_COPY = false;
        // Permission set permission
        public static bool SET_PERMISSION = false;        
        // List major group 
        public static List<string> LST_GROUP;
        // Check is major group change or not
        public static bool IS_CHANGE_MAJOR_GROUP = false;

        // Set right after close window User Report Assign Authorize 
        public static bool IS_RIGHT_CHANGE = false;
        // update print permission to current user
        public static bool IS_PRINT = false;
        // update preview permission to current user
        public static bool IS_VIEW = false;

        // User for change group command
        public static bool IS_CHANGING_GROUP = false;
        
        #region -Enum-
        public enum FilterWindow
        {
            Optional = 1,
            ReportOptional = 2,
            PurchaseOPtional = 3,
            CustomerPaymentOptional = 4
        }
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
            return (dt == DBNull.Value) ? "" : string.Format(DATE_FORMAT, dt);
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
    }
}
