using System.Data;
using CPC.Toolkit.Base;
using CPC.POS.Report.DataSet;
using CPC.POS.Report.CrystalReport;
using CPC.POS.Report.CrystalReport.Guest;
using CPC.POS.Report;
using CPC.POS.Model;
using System.Collections.Generic;
using CPC.POS.Repository;
using System.Collections.ObjectModel;
using CrystalDecisions.CrystalReports.Engine;
using System;
using CPC.Helper;
using System.Linq;
using System.IO;
using CrystalDecisions.Shared;
using SecurityLib;
using System.Net.Mail;
using System.Net;
using Xceed.Wpf.Toolkit;
using CPC.POS.Report.CrystalReport.SO;

namespace CPC.POS.ViewModel
{
    class ReportViewModel : ModelBase
    {
        #region -Properties-
        #region -Report Source-
        protected object _reportSource;
        public object ReportSource
        {
            get { return _reportSource; }
            set
            {
                if (_reportSource != value)
                {
                    _reportSource = value;
                }
            }
        }
        #endregion

        #region -Store model collection-
        /// <summary>
        /// Set or get Store model collection
        /// </summary>
        private List<string> _storeModelCollection;
        public List<string> StoreModelCollection
        {
            get { return _storeModelCollection; }
            set { _storeModelCollection = value; }
        }
        #endregion

        #region -Report name-
        /// <summary>
        /// Set or get Report name
        /// </summary>
        private string _reportTitle;
        public string ReportTitle
        {
            get { return _reportTitle; }
            set { _reportTitle = value; }
        }
        #endregion
        #endregion

        #region -Defines-
        DBHelper dbHelper = new DBHelper();
        PurchaseOrderDataSet purchaseOrderDataSet;
        rptSalesOrder saleOrderReport;
        rptSODetails sODetailsReport;
        rptPickPack pickPackReport;             
        rptGiftCertificateList giftCertificateReport;
        rptPurchaseOrder purchaseOrderReport;
        rptSOReturn sOReturnReport;
        rptCustomerProfile customerProfileReport;
        rptEmployee employeeReport;
        rptVendorProfile vendorProfileReport;
        rptPaymentPlan paymentPlanReport;

        dsSODetails ds = new dsSODetails();
        DataTable dtConfig;
        DataTable dtSO;
        DataTable da;
        string currencySymbol = "'đ'";

        base_StoreRepository storeRepo = new base_StoreRepository();

        byte[] trueImg;

        public const string EMAIL_FORMAT = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";

        public static string POP3_EMAIL_SERVER = string.Empty;
        public static int POP3_PORT_SERVER = 0;
        public static string EMAIL_ACCOUNT = string.Empty;
        public static string EMAIL_PWD = string.Empty;
        #endregion

        #region -Constructor-

        public ReportViewModel(long selectedSaleOrder)
        {
            LoadSalerOrderReport(selectedSaleOrder);
            ReportTitle = "rpt_Sale_Order";
        }

        /// <summary>
        /// Parameter contructor
        /// </summary>
        /// <param name="title"> Report title </param>
        /// <param name="reportName"> Report name</param>
        /// <param name="param">parameter</param>
        /// <param name="isReturn">is return</param>
        /// <param name="email">email address</param>
        /// <param name="SONumber">SO Number</param>
        /// <param name="customerName">customer name</param>
        public ReportViewModel(View.Report.ReportWindow reportView, string reportName, string param = "", string type = "", 
                                Model.base_SaleOrderModel saleOrder = null, Model.base_PurchaseOrderModel purchaseOrder = null)
        {
            switch (reportName)
            {                    
                case "rptSODetails":
                    bool isSendEmail = false;
                    if (reportView == null)
                    {
                        isSendEmail = true;
                    }
                    LoadSalerOrderDetails(saleOrder, type, isSendEmail);
                    ReportTitle = "Sale Order " + saleOrder.SONumber;
                    break;
                case "rptGiftCertificate":
                    LoadGiftCertificateList();
                    ReportTitle = "Card list report";
                    break;
                case "rptPurchaseOrder":
                    LoadPurchaseOrder(purchaseOrder, type);
                    ReportTitle = "Purchase Order " + purchaseOrder.PurchaseOrderNo;
                    break;
                case "rptCustomerProfile":
                    LoadCustomerProfile(param);
                    ReportTitle = "Report Customer Profile";
                    break;
                case "rptEmployee":
                    LoadEmployee(param);
                    ReportTitle = "Report Employee Profile";
                    break;
                case "rptVendorProfile":
                    LoadVendorProfile(param);
                    ReportTitle = "Report Vendor Profile";
                    break;
                case "rptPaymentPlan":
                    LoadPaymentPlan(param);
                    ReportTitle = "Report Payment Plan";
                    break;
            }
            if (reportView != null && ReportSource != null)
            {
                reportView.TotalPage = (ReportSource as ReportDocument).FormatEngine.GetLastPageNumber(new ReportPageRequestContext());
                reportView.SetEnalbeButton();
            }
        }        

        #endregion

        #region -Load Report-

        #region -Load Payment Plan-
        /// <summary>
        /// Load Payment Plan
        /// </summary>
        /// <param name="param">Sale Order Resource</param>
        private void LoadPaymentPlan(string param)
        {
            // Get configuration 
            GetConfiguration();
            da = dbHelper.ExecuteQuery("sp_pos_get_layaway_payment_plan", param);
            int count = da.Rows.Count;
            // count of payment time
            int paymentTime = 0;
            if (count > 0)
            {                
                // Format start date
                DateTime startLayaway = (DateTime)da.Rows[0][0];
                var temStartLayaway = startLayaway;
                // Format end date
                DateTime endLayaway = (DateTime)da.Rows[0][1];                
                var temEndLayaway = endLayaway;
                string billingFrequency = string.Empty;
                int billing = int.Parse(da.Rows[0][8].ToString());
                string name = da.Rows[0][3].ToString();
                string title =  ConvertXMLKeyToName(da.Rows[0][2], "Title");
                if (!string.IsNullOrEmpty(title))
                {
                    name = title + " " + name;
                }
                switch (billing)
                { 
                    case 1:                
                        billingFrequency = "Weekly";
                        while (temStartLayaway.AddDays(7) < temEndLayaway)
                        {
                            temStartLayaway = temStartLayaway.AddDays(7);
                            paymentTime++;
                        }
                        break;
                    case 2:
                        billingFrequency = "Bi-Weekly";
                        while (temStartLayaway.AddDays(14) < temEndLayaway)
                        {
                            temStartLayaway = temStartLayaway.AddDays(14);
                            paymentTime++;
                        }
                        break;
                    case 4:
                        billingFrequency = "Monthly";
                        while (temStartLayaway.AddMonths(1) < temEndLayaway)
                        {
                            temStartLayaway = temStartLayaway.AddMonths(1);
                            paymentTime++;
                        }
                        break;
                    case 3:
                        billingFrequency = "Semi-Monthly";
                        while (temStartLayaway.AddMonths(2) < temEndLayaway)
                        {
                            temStartLayaway = temStartLayaway.AddMonths(2);
                            paymentTime++;
                        }
                        break;
                }
                double total = double.Parse(da.Rows[0][6].ToString());
                double deposit = double.Parse(da.Rows[0][7].ToString());
                ds.Tables["PaymentPlan"].Rows.Add(
                        ToShortDateString(startLayaway), ToShortDateString(endLayaway), name,
                        da.Rows[0][4], da.Rows[0][5], da.Rows[0][6], da.Rows[0][7], billingFrequency
                    );
                double amount = (total -deposit) / paymentTime;
                string paymentDate = string.Empty;
                double balance = 0;
                for (int i = 0; i < paymentTime; i++)
                {
                    switch (billing)
                    {
                        case 1:
                            startLayaway = startLayaway.AddDays(7);
                            paymentDate = ToShortDateString(startLayaway);                           
                            break;
                        case 2:
                            startLayaway = startLayaway.AddDays(14);
                            paymentDate = ToShortDateString(startLayaway);
                            break;
                        case 4:
                            startLayaway = startLayaway.AddMonths(1);
                            paymentDate = ToShortDateString(startLayaway);
                            break;
                        case 3:
                            startLayaway = startLayaway.AddMonths(2);
                            paymentDate = ToShortDateString(startLayaway);
                            break;
                    }
                    balance = (total - deposit) - amount * (i + 1);
                    ds.Tables["PaymentPlanDetails"].Rows.Add(paymentDate, amount, balance);
                }
                paymentPlanReport = new rptPaymentPlan();
                paymentPlanReport.Subreports[0].SetDataSource(ds.Tables["Header"]);
                paymentPlanReport.SetDataSource(ds);
                paymentPlanReport.DataDefinition.FormulaFields["CurrencySymbol"].Text = currencySymbol;
                ReportSource = paymentPlanReport;
            }            
        }
        #endregion

        #region -Load Gift Certificate Report-
        /// <summary>
        /// Get Gift Certificate 
        /// </summary>
        private void LoadGiftCertificateList()
        {
            // Get configuration 
            GetConfiguration();
            // Get all Transfer history details from store        
            da = dbHelper.ExecuteQuery("v_rpt_sale_card_management");
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format purchased date
                string purchasedDate = ToShortDateString(da.Rows[i][2]);
                // Format last userd date
                string lastUsedDate = ToShortDateString(da.Rows[i][4]);
                // Format create date 
                string createdDate = ToShortDateString(da.Rows[i][10]);                
                string paymentMethods = string.Empty;
                if (da.Rows[i][1] != DBNull.Value)
                {
                    // Get payment method 
                    paymentMethods = ConvertXMLKeyToName(da.Rows[i][1], "PaymentMethods");
                }
                string status = string.Empty;
                if (da.Rows[i][8] != DBNull.Value)
                {
                    // Get payment status
                    status = ConvertXMLKeyToName(da.Rows[i][8], "StatusBasic");
                }
                // Convert true false image to byte array
                ConvertImageToByteArray();
                byte[] isSold = null;
                if (da.Rows[i][9] != DBNull.Value && bool.Parse(da.Rows[i][9].ToString()))
                {
                    // Set image show in report
                    isSold = trueImg;
                } 
                // Add data from database to dataset
                ds.Tables["GiftCertificateList"].Rows.Add(
                        da.Rows[i][0], paymentMethods, purchasedDate, da.Rows[i][3],
                        lastUsedDate, da.Rows[i][5], da.Rows[i][6], da.Rows[i][7],
                        status, isSold, createdDate
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set report data source
            giftCertificateReport = new rptGiftCertificateList();
            giftCertificateReport.Subreports[0].SetDataSource(ds.Tables["Header"]);
            giftCertificateReport.SetDataSource(ds.Tables["GiftCertificateList"]);
            giftCertificateReport.DataDefinition.FormulaFields["CurrencySymbol"].Text = currencySymbol;
            if (count == 0)
            {
                // Suppress text in crystal report
                giftCertificateReport.Section4.ReportObjects["Text15"].ObjectFormat.EnableSuppress = true;
                giftCertificateReport.Section3.SectionFormat.EnableSuppress = true;
            }
            ReportSource = giftCertificateReport;
        }
        #endregion

        #region -Load Sale Order Details Report-
        /// <summary>
        /// Load Sale Order Details Report
        /// </summary>
        private void LoadSalerOrderDetails(Model.base_SaleOrderModel saleOrder, string soType, bool isSendEmail)
        {            
            try
            {                
                #region -Get company Configuration-
                string receiptMessage = string.Empty;
                da = dbHelper.ExecuteQuery("v_configuration");
                if (da.Rows.Count > 0)
                {
                    ds.Tables["CompanyInfo"].Rows.Add(da.Rows[0][0], da.Rows[0][1], da.Rows[0][2],
                            PhoneNumberFormat(da.Rows[0][3].ToString()), da.Rows[0][4], da.Rows[0][5], 
                            FaxFormat(da.Rows[0][6].ToString()), da.Rows[0][7]
                        );
                    // Set currency symbol
                    currencySymbol = "'" + da.Rows[0][8].ToString().Trim() + "'";
                    // Set receipt message
                    receiptMessage = da.Rows[0][11].ToString();
                    // Clear data in 
                    da.Clear();
                }
                #endregion
                // set param in sql function
                string param = "'" + saleOrder.Resource.ToString() + "'";                
                bool isReturn = (soType != "Return") ? false : true;                
                // Get Sale order
                GetSaleOrder(saleOrder, isReturn, receiptMessage, soType);
                da.Clear();
                if (soType == "PickPack")
                {
                    GetPickPack(param); 
                    return;
                }
                // Get Sale order details
                bool isEnable = GetSODetails(saleOrder.Id.ToString(), isReturn);
                // Set currency in crystal report
                if (isReturn)
                {
                    sOReturnReport = new rptSOReturn();
                    #region -Suppress control if data is empty-
                    if (!isEnable)
                    {
                        // Suppress control
                        sOReturnReport.ReportDefinition.ReportObjects["lblPaid"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["lblTotal"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["lblBalance"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["lblRewardRefund"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["lblRefund"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["Line1"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["txtPaid"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["txtTotal"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["txtBalance"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["txtRewardRefund"].ObjectFormat.EnableSuppress = false;
                        sOReturnReport.ReportDefinition.ReportObjects["txtRefund"].ObjectFormat.EnableSuppress = false;                        
                    }
                    else
                    {
                        sOReturnReport.Section3.SectionFormat.EnableSuppress = true;
                    }
                    #endregion
                    // Set Report data source
                    sOReturnReport.DataDefinition.FormulaFields["CurrencySymbol"].Text = currencySymbol;
                    sOReturnReport.Subreports[0].SetDataSource(ds.Tables["CompanyInfo"]);
                    sOReturnReport.SetDataSource(ds);
                    ReportSource = sOReturnReport;
                    return;
                }
                else
                {                    
                    if (isEnable)
                    {
                        #region -Suppress control data is empty-
                        // Hide SubTotal
                        sODetailsReport.ReportDefinition.ReportObjects["lblSubTotal"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.ReportDefinition.ReportObjects["txtSubTotal"].ObjectFormat.EnableSuppress = true;
                        // Hide tax
                        sODetailsReport.ReportDefinition.ReportObjects["lblTax"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.ReportDefinition.ReportObjects["txtTax"].ObjectFormat.EnableSuppress = true;
                        // Hide discount
                        sODetailsReport.ReportDefinition.ReportObjects["lblDiscount"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.ReportDefinition.ReportObjects["txtDiscount"].ObjectFormat.EnableSuppress = true;
                        // Hide shipping
                        sODetailsReport.ReportDefinition.ReportObjects["lblShipping"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.ReportDefinition.ReportObjects["txtShipping"].ObjectFormat.EnableSuppress = true;
                        // Hide Tip
                        sODetailsReport.ReportDefinition.ReportObjects["lblTip"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.ReportDefinition.ReportObjects["txtTip"].ObjectFormat.EnableSuppress = true;
                        // Hide Line14 & Line15
                        sODetailsReport.ReportDefinition.ReportObjects["Line14"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.ReportDefinition.ReportObjects["Line15"].ObjectFormat.EnableSuppress = true;
                        // Hide Total
                        sODetailsReport.ReportDefinition.ReportObjects["lblTotal"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.ReportDefinition.ReportObjects["txtTotal"].ObjectFormat.EnableSuppress = true;
                        // Hide Reward
                        sODetailsReport.ReportDefinition.ReportObjects["lblReward"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.ReportDefinition.ReportObjects["txtReward"].ObjectFormat.EnableSuppress = true;
                        // Hide line lnGrandTotal
                        sODetailsReport.ReportDefinition.ReportObjects["lnGrandTotal1"].ObjectFormat.EnableSuppress = true;
                        // Hide Grand Total
                        sODetailsReport.ReportDefinition.ReportObjects["lblGrandTotal"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.ReportDefinition.ReportObjects["txtGrandTotal"].ObjectFormat.EnableSuppress = true;
                        sODetailsReport.Section3.SectionFormat.EnableSuppress = true;
                        #endregion
                    }
                    // Set Report data source
                    sODetailsReport.DataDefinition.FormulaFields["CurrencySymbol"].Text = currencySymbol;
                    sODetailsReport.Subreports[2].DataDefinition.FormulaFields["CurrencySymbol"].Text = currencySymbol;
                    sODetailsReport.Subreports[0].SetDataSource(ds.Tables["CompanyInfo"]);
                    sODetailsReport.Subreports[1].SetDataSource(ds.Tables["Payment"]);
                    sODetailsReport.Subreports[2].SetDataSource(ds.Tables["Reward"]);
                    sODetailsReport.SetDataSource(ds);
                    ReportSource = sODetailsReport;                    
                    if (isSendEmail)
                    {
                        #region -Send email to customer-
                        // Get customer name
                        string Title = ConvertXMLKeyToName(saleOrder.GuestModel.Title, "Title") != "" ? ConvertXMLKeyToName(saleOrder.GuestModel.Title, "Title") + " " : "";
                        string customerName = Title + saleOrder.GuestModel.LastName + " " + saleOrder.GuestModel.FirstName + " " + saleOrder.GuestModel.MiddleName;
                        // Get config to send email
                        GetEmailConfig();
                        string attachFile = ExportPDFFile(saleOrder.SONumber);
                        string subject = "Receipt " + saleOrder.SONumber + " from SmartPOS";
                        // Send email to customer
                        string errorList = SendEmail(saleOrder.GuestModel.Email, subject, attachFile, customerName);
                        // Show error list
                        if (!string.IsNullOrEmpty(errorList))
                        {
                            MessageBox.Show("Error list: " + errorList.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        #endregion
                    }
                }                
            }
            catch (System.Exception ex)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }), System.Windows.Threading.DispatcherPriority.Normal);
                
                return;
            }
        }        

        #region -Export pdf file-
        /// <summary>
        /// Export report to pdf file
        /// </summary>
        /// <param name="soNumber"></param>
        /// <returns>Full path export file name</returns>
        private string ExportPDFFile(string soNumber)
        {
            // Export to pdf file
            ExportOptions CrExportOptions = new ExportOptions();
            DiskFileDestinationOptions CrDiskFileDestinationOptions = new DiskFileDestinationOptions();
            // Create folder to store report file (pdf file)
            string folderName = Directory.GetCurrentDirectory() + "\\PDF File\\rptSODetails";
            if (!Directory.Exists(folderName))
            {
                // Create forder if forder name does not exist
                Directory.CreateDirectory(folderName);
            }
            // Set full path file name
            string exportFile = folderName + "\\SmartPOS Receipt " + soNumber + ".pdf";
            CrDiskFileDestinationOptions.DiskFileName = exportFile;
            PdfRtfWordFormatOptions CrFormatTypeOptions = new PdfRtfWordFormatOptions();
            // Export to pdf file
            CrExportOptions = sODetailsReport.ExportOptions;
            CrExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
            CrExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
            CrExportOptions.DestinationOptions = CrDiskFileDestinationOptions;
            CrExportOptions.FormatOptions = CrFormatTypeOptions;
            sODetailsReport.Export();
            return exportFile;
        }
        #endregion

        #region -Get Sale Order-
        /// <summary>
        /// Get sale order
        /// </summary>
        /// <param name="saleOrder"></param>
        /// <param name="isReturn"></param>
        /// <param name="receiptMessage"></param>
        private void GetSaleOrder(Model.base_SaleOrderModel saleOrder, bool isReturn, string receiptMessage, string type)
        {
            // Set param in sql function
            string resource = "'" + saleOrder.Resource.ToString() + "'";
            string param = string.Format("{0},{1}", resource, isReturn);
            bool isChangeTax = false;
            bool isHideTip = false;
            bool isHideReward = false;
            bool isHideRemark = false;            
            // Get Sale Order
            da = dbHelper.ExecuteQuery("sp_pos_so_get_sale_order", param);
            if (da.Rows.Count > 0)
            {
                // Format order date
                string orderDate = ToShortDateString(DateTime.Parse(da.Rows[0][3].ToString()));
                // Add data to data table
                ds.Tables["SO"].Rows.Add(GetStoreNameByStoreCode(int.Parse(da.Rows[0][0].ToString())),
                            da.Rows[0][1], da.Rows[0][2], orderDate, da.Rows[0][4], da.Rows[0][5], da.Rows[0][6],
                            da.Rows[0][7], da.Rows[0][8], da.Rows[0][9], da.Rows[0][10], da.Rows[0][11],
                            receiptMessage, da.Rows[0][12], da.Rows[0][13], da.Rows[0][14]
                        );
                isChangeTax = double.Parse(da.Rows[0][7].ToString()) != 0.0;
                isHideTip = double.Parse(da.Rows[0][10].ToString()) == 0.0;
                isHideRemark = da.Rows[0][12] == DBNull.Value;
                isHideReward = (bool)da.Rows[0][13];
            }
            else
            {
                // add data to data table
                ds.Tables["SO"].Rows.Add(
                            GetStoreNameByStoreCode(saleOrder.StoreCode), saleOrder.SOCardImg, saleOrder.SONumber,
                            ToShortDateString(saleOrder.OrderDate), saleOrder.UserCreated, saleOrder.SubTotal,
                            saleOrder.TaxCode, saleOrder.DiscountAmount, saleOrder.Shipping,
                            saleOrder.Total, 0, saleOrder.TaxAmount, receiptMessage,
                            saleOrder.Remark, saleOrder.IsRedeeem, saleOrder.RewardAmount
                        );
                isChangeTax = saleOrder.DiscountAmount != (decimal)0.0;
                isHideTip = true;
                isHideRemark = string.IsNullOrEmpty(saleOrder.Remark);
                isHideReward = saleOrder.IsRedeeem;
            }
            if (!isReturn)
            {
                sODetailsReport = new rptSODetails();
                if (type == "SaleOrder")
                {                                        
                    #region -Hide lalbel and text-
                    // Hide tax
                    sODetailsReport.ReportDefinition.ReportObjects["lblTax"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["txtTax"].ObjectFormat.EnableSuppress = true;
                    // Hide discount
                    sODetailsReport.ReportDefinition.ReportObjects["lblDiscount"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["txtDiscount"].ObjectFormat.EnableSuppress = true;
                    // Hide shipping
                    sODetailsReport.ReportDefinition.ReportObjects["lblShipping"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["txtShipping"].ObjectFormat.EnableSuppress = true;
                    // Hide Tip
                    sODetailsReport.ReportDefinition.ReportObjects["lblTip"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["txtTip"].ObjectFormat.EnableSuppress = true;
                    // Hide Line14 & Line15
                    sODetailsReport.ReportDefinition.ReportObjects["Line14"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["Line15"].ObjectFormat.EnableSuppress = true;
                    // Hide Total
                    sODetailsReport.ReportDefinition.ReportObjects["lblTotal"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["txtTotal"].ObjectFormat.EnableSuppress = true;
                    // Hide Reward
                    sODetailsReport.ReportDefinition.ReportObjects["lblReward"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["txtReward"].ObjectFormat.EnableSuppress = true;
                    // Hide line lnGrandTotal
                    sODetailsReport.ReportDefinition.ReportObjects["lnGrandTotal1"].ObjectFormat.EnableSuppress = true;
                    // Hide Grand Total
                    sODetailsReport.ReportDefinition.ReportObjects["lblGrandTotal"].ObjectFormat.EnableSuppress = true;                    
                    sODetailsReport.ReportDefinition.ReportObjects["txtGrandTotal"].ObjectFormat.EnableSuppress = true;
                    // Rename Title
                    ((TextObject)sODetailsReport.ReportDefinition.ReportObjects["txtTitle"]).Text = "SALE ORDER";
                    #endregion
                }
                else
                {
                    // Supppress control in crystal report
                    SuppressControl(isChangeTax, isHideTip, isHideReward);
                    if (type == "Receipt")
                    {                        
                        // Get resource payment
                        GetPayment(resource);
                        sODetailsReport.ReportDefinition.ReportObjects["lblReceiptMessage"].ObjectFormat.EnableSuppress = false;
                        da = dbHelper.ExecuteQuery("sp_pos_get_reward", resource);
                        if (da.Rows.Count == 0)
                        {
                            ds.Tables["Reward"].Rows.Add();
                        }
                        else
                        {
                            string exp = (da.Rows[0][3] != DBNull.Value) ? ToShortDateString(da.Rows[0][3]) : "Never";
                            ds.Tables["Reward"].Rows.Add(da.Rows[0][0], da.Rows[0][1],
                                    da.Rows[0][2], exp, ToShortDateString(da.Rows[0][4])
                                   );
                            sODetailsReport.ReportDefinition.ReportObjects["rewardSubReport"].ObjectFormat.EnableSuppress = false;
                        }
                        da.Clear();
                    }
                    else
                    {
                        // Rename title
                        ((TextObject)sODetailsReport.ReportDefinition.ReportObjects["txtTitle"]).Text = "SALE INVOICE";                        
                    }
                }
                if (isHideRemark)
                {
                    // Hide Remark
                    sODetailsReport.ReportDefinition.ReportObjects["txtRemark"].ObjectFormat.EnableSuppress = true;
                }    
            }
        }
        #endregion

        #region -Suppress control-
        /// <summary>
        /// Hide control in crystal report
        /// </summary>
        /// <param name="isChangeTax">Flash to check whether is change label Tax and discount or not</param>
        /// <param name="isHideTip">Flash to check whether is hide tip or not</param>
        /// <param name="isHideRemark">Flash to check whether is hide remark or not</param>
        /// <param name="isHideReward">Flash to check whether is hide reward or not</param>
        private void SuppressControl(bool isChangeTax, bool isHideTip, bool isHideReward)
        {            
            if (isChangeTax)
            {
                // Change label name from "Discount" to "Tax"
                ((TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblDiscount"]).Text = "Tax";
                // Change label name from "Tax" to "TDiscount"
                ((TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblTax"]).Text = "Discount";
            }                                
            if (isHideTip && isHideReward == false)
            {
                // Hide Tip
                sODetailsReport.ReportDefinition.ReportObjects["lblTip"].ObjectFormat.EnableSuppress = true;
                sODetailsReport.ReportDefinition.ReportObjects["txtTip"].ObjectFormat.EnableSuppress = true;
                // Hide Reward
                sODetailsReport.ReportDefinition.ReportObjects["lblReward"].ObjectFormat.EnableSuppress = true;
                sODetailsReport.ReportDefinition.ReportObjects["txtReward"].ObjectFormat.EnableSuppress = true;
                // Hide line lnGrandTotal1
                sODetailsReport.ReportDefinition.ReportObjects["lnGrandTotal1"].ObjectFormat.EnableSuppress = true;
                // Show line lnGrandTotal2
                sODetailsReport.ReportDefinition.ReportObjects["lnGrandTotal3"].ObjectFormat.EnableSuppress = false;
                // Set new possition for lblGrandTotal
                ((CrystalDecisions.CrystalReports.Engine.TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblGrandTotal"]).Top = 1550;
                ((CrystalDecisions.CrystalReports.Engine.TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblGrandTotal"]).Left = 7770;
                // Set new possition for txtGrandTotal
                ((CrystalDecisions.CrystalReports.Engine.FieldObject)sODetailsReport.ReportDefinition.ReportObjects["txtGrandTotal"]).Top = 1580;
                ((CrystalDecisions.CrystalReports.Engine.FieldObject)sODetailsReport.ReportDefinition.ReportObjects["txtGrandTotal"]).Left = 9430;
            }
            else
            {
                // Hide Tip and change orther object's position in report
                if (isHideTip)
                {
                    // Hide Tip
                    sODetailsReport.ReportDefinition.ReportObjects["lblTip"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["txtTip"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["lnGrandTotal1"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["lnGrandTotal2"].ObjectFormat.EnableSuppress = false;
                    // Reset postion
                    ((CrystalDecisions.CrystalReports.Engine.TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblReward"]).Top = 1500;
                    ((CrystalDecisions.CrystalReports.Engine.TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblReward"]).Left = 7770;
                    ((CrystalDecisions.CrystalReports.Engine.FieldObject)sODetailsReport.ReportDefinition.ReportObjects["txtReward"]).Top = 1530;
                    ((CrystalDecisions.CrystalReports.Engine.FieldObject)sODetailsReport.ReportDefinition.ReportObjects["txtReward"]).Left = 9430;
                    ((CrystalDecisions.CrystalReports.Engine.TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblGrandTotal"]).Top = 1850;
                    ((CrystalDecisions.CrystalReports.Engine.TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblGrandTotal"]).Left = 7770;
                    ((CrystalDecisions.CrystalReports.Engine.FieldObject)sODetailsReport.ReportDefinition.ReportObjects["txtGrandTotal"]).Top = 1880;
                    ((CrystalDecisions.CrystalReports.Engine.FieldObject)sODetailsReport.ReportDefinition.ReportObjects["txtGrandTotal"]).Left = 9430;
                }

                // Hide Reward and change orther object's position in report
                if (isHideReward == false)
                {
                    sODetailsReport.ReportDefinition.ReportObjects["lblReward"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["txtReward"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["lnGrandTotal1"].ObjectFormat.EnableSuppress = true;
                    sODetailsReport.ReportDefinition.ReportObjects["lnGrandTotal2"].ObjectFormat.EnableSuppress = false;
                    ((CrystalDecisions.CrystalReports.Engine.TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblGrandTotal"]).Top = 1850;
                    ((CrystalDecisions.CrystalReports.Engine.TextObject)sODetailsReport.ReportDefinition.ReportObjects["lblGrandTotal"]).Left = 7770;
                    ((CrystalDecisions.CrystalReports.Engine.FieldObject)sODetailsReport.ReportDefinition.ReportObjects["txtGrandTotal"]).Top = 1880;
                    ((CrystalDecisions.CrystalReports.Engine.FieldObject)sODetailsReport.ReportDefinition.ReportObjects["txtGrandTotal"]).Left = 9430;
                }
            }
        }
        #endregion

        #region -Get Sale Order Details-
        /// <summary>
        /// Get Sale Order Details
        /// </summary>
        /// <param name="soId"></param>
        /// <param name="isReturn"></param>
        private bool GetSODetails(string soId, bool isReturn)
        {
            // Set param in sql function sp_pos_so_get_sale_order_details
            string isReturnParam = isReturn.ToString();
            da = dbHelper.ExecuteQuery("sp_pos_so_get_sale_order_details", string.Format("{0}, {1}", soId, isReturn));
            if (da.Rows.Count > 0)
            {
                ConvertImageToByteArray();                
                for (int i = 0; i < da.Rows.Count; i++)
                {
                    byte[] isReturned = null;
                    if (da.Rows[i][6] != DBNull.Value && bool.Parse(da.Rows[i][6].ToString()))                    
                    {
                        // Set image show in report
                        isReturned = trueImg;
                    } 
                    // Add data to data table 
                    ds.Tables["SODetails"].Rows.Add(
                            da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], da.Rows[i][3], 
                            da.Rows[i][4], da.Rows[i][5], isReturned
                        );
                }
                da.Clear();
                return false;
            }
            else
            {
                ds.Tables["SODetails"].Rows.Add();
            }
            return true;
        }
        #endregion

        #region -Get resource payment-
        /// <summary>
        /// Get resource payment
        /// </summary>
        /// <param name="param"></param>
        private void GetPayment(string param)
        {
            // Get resource payment from database
            da = dbHelper.ExecuteQuery("sp_pos_so_resource_payment_by_resource", param);
            if (da.Rows.Count > 0)
            {
                bool isSuppressSignature = true;
                for (int i = 0; i < da.Rows.Count; i++)
                {
                    // Add to data table 
                    ds.Tables["Payment"].Rows.Add(
                        da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], da.Rows[i][3]
                        );
                    // Payment type <> cash.
                    if (int.Parse(da.Rows[i][1].ToString()) != 0)
                    {
                        isSuppressSignature = false;
                    }
                }
                // Set currency symbol
                sODetailsReport.Subreports[1].DataDefinition.FormulaFields["CurrencySymbol"].Text = currencySymbol;
                sODetailsReport.Subreports[1].ReportDefinition.ReportObjects["txtSignature"].ObjectFormat.EnableSuppress = isSuppressSignature;
                da.Clear();
            }
        }
        #endregion

        #region -Get Pick Pack-

        private void GetPickPack(string resourse)
        {
            pickPackReport = new rptPickPack();
            da = dbHelper.ExecuteQuery("sp_pos_so_get_pick_pack", resourse);
            if (da.Rows.Count > 0)
            {
                ConvertImageToByteArray();
                for (int i = 0; i < da.Rows.Count; i++)
                {
                    byte[] isReturned = null;
                    if (da.Rows[i][5] != DBNull.Value && bool.Parse(da.Rows[i][5].ToString()))
                    {
                        // Set image show in report
                        isReturned = trueImg;
                    }
                    // Add data to data table 
                    ds.Tables["PickPack"].Rows.Add(
                            da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], da.Rows[i][3], da.Rows[i][4], 
                            isReturned, ToShortDateString(da.Rows[i][6]), da.Rows[i][7]
                        );
                }
                da.Clear();
            }
            else
            {
                ds.Tables["PickPack"].Rows.Add();
                pickPackReport.ReportDefinition.ReportObjects["lblGroup"].ObjectFormat.EnableSuppress = true;
                pickPackReport.ReportDefinition.ReportObjects["lblGrandTotal"].ObjectFormat.EnableSuppress = true;
                pickPackReport.Section3.SectionFormat.EnableSuppress = true;
            }
            // Set Report data source            
            pickPackReport.Subreports[0].SetDataSource(ds.Tables["CompanyInfo"]);
            pickPackReport.SetDataSource(ds);
            ReportSource = pickPackReport;   
        }
        #endregion

        #endregion

        #region Load Purchase Order
        /// <summary>
        /// Load Purchase Order Report.
        /// </summary>
        private void LoadPurchaseOrder(Model.base_PurchaseOrderModel purchaseOrder, string type)
        {
            try
            {
                bool isReturn = (type != "PReturn") ? false : true;
                // Set param to sql function
                string param = purchaseOrder.Id.ToString() + "," + isReturn.ToString();
                purchaseOrderDataSet = new PurchaseOrderDataSet();                
                purchaseOrderReport = new rptPurchaseOrder();
                // Get store name
                TextObject storeName = purchaseOrderReport.ReportDefinition.ReportObjects["StoreNumber"] as TextObject;
                if (storeName != null)
                {
                    storeName.Text = GetStoreNameByStoreCode(Define.StoreCode);
                }
                // Get configuration
                da = dbHelper.ExecuteQuery("v_configuration");
                int count = da.Rows.Count;
                if (count > 0)
                {
                    purchaseOrderDataSet.Tables["CompanyInfo"].Rows.Add(
                            da.Rows[0][0], da.Rows[0][1], da.Rows[0][2], 
                            PhoneNumberFormat(da.Rows[0][3].ToString()), da.Rows[0][4], 
                            da.Rows[0][5], FaxFormat(da.Rows[0][6].ToString()), da.Rows[0][8]
                        );
                }                
                // Get purchase order 
                da = dbHelper.ExecuteQuery("sp_pos_get_purchase_order", param);
                count = da.Rows.Count;
                if (count > 0)
                {
                    purchaseOrderDataSet.Tables["PurchaseOrder"].Rows.Add(
                            da.Rows[0][0], ToShortDateString(da.Rows[0][1]), da.Rows[0][2], 
                            da.Rows[0][3], da.Rows[0][4], da.Rows[0][5], da.Rows[0][6]
                        );
                }
                else
                {
                    purchaseOrderDataSet.Tables["PurchaseOrder"].Rows.Add(
                            purchaseOrder.PurchaseOrderNo, ToShortDateString(purchaseOrder.PurchasedDate),
                            purchaseOrder.PaymentTermDescription, purchaseOrder.POCardImg, 0, 0, 0
                        );
                }
                // Get Purchase order details
                da = dbHelper.ExecuteQuery("sp_pos_get_purchase_order_details", param);
                count = da.Rows.Count;
                if (count > 0)
                {                                        
                    if (type == "PReturn")
                    {
                        ConvertImageToByteArray();                        
                        for (int i = 0; i < count; i++)
                        {
                            byte[] imgReturn = null;
                            if (da.Rows[i][8] != DBNull.Value && bool.Parse(da.Rows[i][8].ToString()))
                            {
                                // Set image show in report
                                imgReturn =  trueImg;
                            }
                            purchaseOrderDataSet.Tables["PurchaseOrderDetail"].Rows.Add(
                                        da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], da.Rows[i][3],
                                        da.Rows[i][4], da.Rows[i][5], da.Rows[i][6], da.Rows[i][7], imgReturn
                                );
                        }
                        // Clear data in table 
                        da.Clear();
                        #region -Change title and suppress control-
                        (purchaseOrderReport.ReportDefinition.ReportObjects["Text6"] as TextObject).Text = "PURCHASE ORDER RETURN";
                        (purchaseOrderReport.ReportDefinition.ReportObjects["lblTotal"] as TextObject).Text = "Sub-Total";
                        (purchaseOrderReport.ReportDefinition.ReportObjects["lblPaid"] as TextObject).Text = "Return Fee";
                        (purchaseOrderReport.ReportDefinition.ReportObjects["lblBalance"] as TextObject).Text = "Refund";
                        purchaseOrderReport.ReportDefinition.ReportObjects["lblBalance1"].ObjectFormat.EnableSuppress = false;
                        purchaseOrderReport.ReportDefinition.ReportObjects["txtBalance1"].ObjectFormat.EnableSuppress = false;
                        purchaseOrderReport.ReportDefinition.ReportObjects["lnIsReturn"].ObjectFormat.EnableSuppress = false;
                        purchaseOrderReport.ReportDefinition.ReportObjects["lblReturn"].ObjectFormat.EnableSuppress = false;
                        purchaseOrderReport.ReportDefinition.ReportObjects["txtIsReturn"].ObjectFormat.EnableSuppress = false;
                        purchaseOrderReport.ReportDefinition.ReportObjects["lblAmount"].Width = 1560;
                        purchaseOrderReport.ReportDefinition.ReportObjects["txtAmount"].Width = 1560;
                        #endregion
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            purchaseOrderDataSet.Tables["PurchaseOrderDetail"].Rows.Add(
                                        da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], da.Rows[i][3],
                                        da.Rows[i][4], da.Rows[i][5], da.Rows[i][6], da.Rows[i][7]
                                );
                        } da.Dispose();
                        // Clear data in table 
                        da.Clear();
                        if (type == "POrder")
                        {
                            #region -Suppress control-
                            (purchaseOrderReport.ReportDefinition.ReportObjects["Text6"] as TextObject).Text = "PURCHASE ORDER";
                            purchaseOrderReport.ReportDefinition.ReportObjects["lblPaid"].ObjectFormat.EnableSuppress = true;
                            purchaseOrderReport.ReportDefinition.ReportObjects["txtPaid"].ObjectFormat.EnableSuppress = true;
                            purchaseOrderReport.ReportDefinition.ReportObjects["lblBalance"].ObjectFormat.EnableSuppress = true;
                            purchaseOrderReport.ReportDefinition.ReportObjects["txtBalance"].ObjectFormat.EnableSuppress = true;
                            purchaseOrderReport.ReportDefinition.ReportObjects["lblBalance1"].ObjectFormat.EnableSuppress = true;
                            purchaseOrderReport.ReportDefinition.ReportObjects["txtBalance1"].ObjectFormat.EnableSuppress = true;
                            purchaseOrderReport.ReportDefinition.ReportObjects["txtTotal"].Left = 9320;
                            #endregion
                        }
                        else
                        {
                            purchaseOrderReport.ReportDefinition.ReportObjects["txtTotal"].Left = 9320;
                            purchaseOrderReport.ReportDefinition.ReportObjects["txtPaid"].Left = 9320;
                            purchaseOrderReport.ReportDefinition.ReportObjects["txtBalance"].Left = 9320;
                        }
                    }
                }
                else
                {
                    #region -Suppess control-
                    purchaseOrderReport.ReportDefinition.ReportObjects["lblTotal"].ObjectFormat.EnableSuppress = true;
                    purchaseOrderReport.ReportDefinition.ReportObjects["txtTotal"].ObjectFormat.EnableSuppress = true;
                    purchaseOrderReport.ReportDefinition.ReportObjects["lblPaid"].ObjectFormat.EnableSuppress = true;
                    purchaseOrderReport.ReportDefinition.ReportObjects["txtPaid"].ObjectFormat.EnableSuppress = true;
                    purchaseOrderReport.ReportDefinition.ReportObjects["lblBalance"].ObjectFormat.EnableSuppress = true;
                    purchaseOrderReport.ReportDefinition.ReportObjects["txtBalance"].ObjectFormat.EnableSuppress = true;
                    purchaseOrderReport.ReportDefinition.ReportObjects["lblBalance1"].ObjectFormat.EnableSuppress = true;
                    purchaseOrderReport.ReportDefinition.ReportObjects["txtBalance1"].ObjectFormat.EnableSuppress = true;
                    purchaseOrderReport.ReportDefinition.ReportObjects["Line4"].ObjectFormat.EnableSuppress = true;
                    purchaseOrderReport.Section3.SectionFormat.EnableSuppress = true;
                    if (type == "POrder")
                    {
                        (purchaseOrderReport.ReportDefinition.ReportObjects["Text6"] as TextObject).Text = "PURCHASE ORDER";
                    }
                    else if (type == "PReturn")
                    {
                        (purchaseOrderReport.ReportDefinition.ReportObjects["Text6"] as TextObject).Text = "PURCHASE ORDER RETURN";
                        purchaseOrderReport.ReportDefinition.ReportObjects["lblReturn"].ObjectFormat.EnableSuppress = false;
                        purchaseOrderReport.ReportDefinition.ReportObjects["lnIsReturn"].ObjectFormat.EnableSuppress = false;
                        purchaseOrderReport.ReportDefinition.ReportObjects["lblAmount"].Width = 1500;
                        purchaseOrderReport.ReportDefinition.ReportObjects["txtAmount"].Width = 1500;
                    }
                    #endregion
                }
                // Set data source
                purchaseOrderReport.SetDataSource(purchaseOrderDataSet);
                ReportSource = purchaseOrderReport;
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region -Load customer profile-
        /// <summary>
        /// Load Customer Profile 
        /// </summary>
        /// <param name="param"></param>
        private void LoadCustomerProfile(string param)
        {
            // Get customer profile by resource
            da = dbHelper.ExecuteQuery("sp_pos_get_customer_profile", param);
            if (da.Rows.Count > 0)
            {
                // Concat Title and name for customer name
                string customerName = da.Rows[0][2].ToString();
                string title = ConvertXMLKeyToName(da.Rows[0][1], "Title");
                if (!string.IsNullOrWhiteSpace(title))
                {
                    customerName = title + " " + customerName;
                }
                string emergencyName = da.Rows[0][20].ToString();
                // Concat Title and name for emegenct contact
                title = ConvertXMLKeyToName(da.Rows[0][19], "Title");
                if (!string.IsNullOrWhiteSpace(title))
                {
                    emergencyName = title + " " + emergencyName;
                }
                // Get states name
                string billState = ConvertXMLKeyToName(da.Rows[0][11], "states");
                string shipState = ConvertXMLKeyToName(da.Rows[0][16], "states");
                // Get Country name
                string billCountry = ConvertXMLKeyToName(da.Rows[0][13], "country");
                string shipCountry = ConvertXMLKeyToName(da.Rows[0][18], "country");
                // Format phone number
                string cPhone = PhoneNumberFormat(da.Rows[0][3].ToString());
                string cCellPhone = PhoneNumberFormat(da.Rows[0][7].ToString());
                string ePhone = PhoneNumberFormat(da.Rows[0][21].ToString());
                string eCellPhone = PhoneNumberFormat(da.Rows[0][22].ToString());
                // Add data to data set
                ds.Tables["CustomerProfile"].Rows.Add(
                        da.Rows[0][0], customerName, cPhone, da.Rows[0][4],
                        da.Rows[0][5], da.Rows[0][6], cCellPhone, da.Rows[0][8], da.Rows[0][9],
                        da.Rows[0][10], billState, ZipCodeFormat(da.Rows[0][12].ToString()), billCountry, da.Rows[0][14],
                        da.Rows[0][15], shipState, ZipCodeFormat(da.Rows[0][17].ToString()), shipCountry, emergencyName,
                        ePhone, eCellPhone, da.Rows[0][23], da.Rows[0][24]
                    );
                // Clear data in table 
                da.Clear();
            }
            // Set data source
            customerProfileReport = new rptCustomerProfile();
            customerProfileReport.SetDataSource(ds.Tables["CustomerProfile"]);
            ReportSource = customerProfileReport;
        }
        #endregion

        #region -Load Empoyee information- 
        /// <summary>
        /// Load Empoyee information
        /// </summary>
        /// <param name="param"></param>
        private void LoadEmployee(string param)
        {
            // Get customer profile by resource
            da = dbHelper.ExecuteQuery("sp_pos_get_employee_information", param);
            if (da.Rows.Count > 0)
            {
                // Concat Employee name
                string employeeName = da.Rows[0][2].ToString();
                string title = ConvertXMLKeyToName(da.Rows[0][1], "Title");
                if (!string.IsNullOrWhiteSpace(title))
                {
                    employeeName = title + " " + employeeName;
                }
                // Concat Orther information name
                title = ConvertXMLKeyToName(da.Rows[0][14], "Title");
                string sEmployeeName = da.Rows[0][15].ToString();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    sEmployeeName = title + " " + sEmployeeName;
                }
                // Concat Emegency contact name
                title = ConvertXMLKeyToName(da.Rows[0][24], "Title");
                string emegencyName = da.Rows[0][25].ToString();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    emegencyName = title + " " + emegencyName;
                }
                // Get city & state name
                string cityState = da.Rows[0][4].ToString();
                string state = ConvertXMLKeyToName(da.Rows[0][5], "states");
                if (!string.IsNullOrWhiteSpace(state) || da.Rows[0][6] != DBNull.Value)
                {
                    cityState += ", " + state + " " + ZipCodeFormat(da.Rows[0][6].ToString());
                }
                // Get country name
                string country = ConvertXMLKeyToName(da.Rows[0][7], "country");
                // Get marital status name
                string maritalStatus = ConvertXMLKeyToName(da.Rows[0][13], "MaritalStatus");
                // Format date time
                string DOB = ToShortDateString(da.Rows[0][12]);
                string otherDOB = ToShortDateString(da.Rows[0][17]);
                string phone = PhoneNumberFormat(da.Rows[0][8].ToString());
                string cellPhone = PhoneNumberFormat(da.Rows[0][9].ToString());
                string sPhone = PhoneNumberFormat(da.Rows[0][20].ToString());
                string sCellPhone = PhoneNumberFormat(da.Rows[0][21].ToString());
                string ePhone = PhoneNumberFormat(da.Rows[0][26].ToString());
                string eCellPhone = PhoneNumberFormat(da.Rows[0][27].ToString());
                // Add data to data set
                ds.Tables["Employee"].Rows.Add(
                        da.Rows[0][0], employeeName, da.Rows[0][3], cityState, country,
                        phone, cellPhone, da.Rows[0][10], da.Rows[0][11],
                        DOB, maritalStatus, sEmployeeName, da.Rows[0][16], otherDOB,
                        da.Rows[0][18], da.Rows[0][19], sPhone, sCellPhone, da.Rows[0][22],
                        da.Rows[0][23], emegencyName, ePhone, eCellPhone, da.Rows[0][28]
                    );
                // Clear data in table 
                da.Clear();
            }
            // Set report data source
            employeeReport = new rptEmployee();
            employeeReport.SetDataSource(ds.Tables["Employee"]);
            ReportSource = employeeReport;
        }
        #endregion     

        #region -Load Vendor profile-
        /// <summary>
        /// Load Customer Profile 
        /// </summary>
        /// <param name="param"></param>
        private void LoadVendorProfile(string param)
        {
            // Get customer profile by resource
            da = dbHelper.ExecuteQuery("sp_pos_get_vendor_profile", param);
            if (da.Rows.Count > 0)
            {
                // Get states name
                string state = ConvertXMLKeyToName(da.Rows[0][5], "states");
                // Get Country name
                string country = ConvertXMLKeyToName(da.Rows[0][7], "country");
                string cityState = da.Rows[0][4].ToString();
                if (!string.IsNullOrEmpty(state) || DBNull.Value != da.Rows[0][6])
                {
                    cityState += ", " + state + " " + ZipCodeFormat(da.Rows[0][6].ToString());
                }
                // Format phone number
                string phone1 = PhoneNumberFormat(da.Rows[0][9].ToString());
                string phone2 = PhoneNumberFormat(da.Rows[0][10].ToString());
                string cellPhone = PhoneNumberFormat(da.Rows[0][11].ToString());
                string fax = FaxFormat(da.Rows[0][12].ToString());
                // Add data to data set
                ds.Tables["Vendor"].Rows.Add(
                        da.Rows[0][0], da.Rows[0][1], da.Rows[0][2], da.Rows[0][3], 
                        cityState, country, da.Rows[0][8], phone1, phone2, cellPhone, 
                        fax, da.Rows[0][13], da.Rows[0][14], FedTaxIdFormat(da.Rows[0][15].ToString()), 
                        da.Rows[0][16], da.Rows[0][17], da.Rows[0][18]
                    );
                // Clear data in table 
                da.Clear();
            }
            // Set data source
            vendorProfileReport = new rptVendorProfile();
            vendorProfileReport.SetDataSource(ds.Tables["Vendor"]);
            ReportSource = vendorProfileReport;
        }
        #endregion 

        #endregion

        #region -Private method-

        #region -Phone number format-
        /// <summary>
        /// Format phone nubmer like (###) ###-####
        /// </summary>
        /// <param name="phoneNumber">Phone to format</param>
        /// <returns></returns>
        private string PhoneNumberFormat(string phoneNumber)
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

        #region -Fax format-
        /// <summary>
        /// Format Fax like ###-###-####
        /// </summary>
        /// <param name="phoneNumber">Fax to format</param>
        /// <returns></returns>
        private string FaxFormat(string fax)
        {
            if (fax.Length == 10)
            {
                return string.Format("{0}-{1}-{2}",
                                            fax.Substring(0, 3),
                                            fax.Substring(3, 3),
                                            fax.Substring(6)
                                        );
            }
            return fax;
        }
        #endregion

        #region -Zip code format-
        /// <summary>
        /// Format Zip code like #####-####
        /// </summary>
        /// <param name="phoneNumber">Zip code to format</param>
        /// <returns></returns>
        private string ZipCodeFormat(string zipCode)
        {
            if (zipCode.Length == 9)
            {
                return string.Format("{0}-{1}", zipCode.Substring(0, 5), zipCode.Substring(5));
            }
            return zipCode;
        }
        #endregion

        #region -Fed Tax Id format-
        /// <summary>
        /// Format Fed Tax Id like ##-#######
        /// </summary>
        /// <param name="phoneNumber">Fex tax Id to format</param>
        /// <returns></returns>
        private string FedTaxIdFormat(string fedTaxId)
        {
            if (fedTaxId.Length == 9)
            {
                return string.Format("{0}-{1}", fedTaxId.Substring(0, 2), fedTaxId.Substring(2));
            }
            return fedTaxId;
        }
        #endregion

        #region -Get email config-
        /// <summary>
        /// Get Email Config
        /// </summary>
        private void GetEmailConfig()
        {
            // Get email config
            da = dbHelper.ExecuteQuery("v_get_email_config");
            bool isNullEmailConfig = string.IsNullOrEmpty(da.Rows[0][0].ToString()) || string.IsNullOrEmpty(da.Rows[0][1].ToString()) || string.IsNullOrEmpty(da.Rows[0][2].ToString()) || string.IsNullOrEmpty(da.Rows[0][3].ToString());
            if (da.Rows.Count <= 0 || isNullEmailConfig)
            {
                throw new Exception("Email not configured");
            }
            // Set server's Email
            POP3_EMAIL_SERVER = da.Rows[0][0].ToString();
            // Set port to send email
            POP3_PORT_SERVER = int.Parse(da.Rows[0][1].ToString());
            // Set Email account to send email
            EMAIL_ACCOUNT = da.Rows[0][2].ToString();
            // password form email account
            EMAIL_PWD = da.Rows[0][3].ToString();
            da.Clear();         
        }
        #endregion

        #region -Send email to customer-
        /// <summary>
        /// Send email function
        /// </summary>
        /// <param name="mailTo">To email address</param>
        /// <param name="subject">Subject of email</param>
        /// <param name="attachmentFile">Attach file</param>
        /// <param name="customerName">Customer name</param>
        /// <returns>Error list </returns>
        public static string SendEmail(string mailTo, string subject, string attachmentFile, string customerName)
        {
            // Checks content to send.
            if (string.IsNullOrWhiteSpace(Define.EmailContentSendReportFile))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Email content send report  file not found.", Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return string.Empty;
            }
            string body = string.Empty;
            // Read Mail templete from text file
            using (StreamReader reader = new StreamReader(Define.EmailContentSendReportFile))
            {
                body = reader.ReadToEnd();
            }
            string errorList = string.Empty;
            // Decrypt password
            string pwd = AESSecurity.Decrypt(EMAIL_PWD);
            SmtpClient smtpClient = new SmtpClient(POP3_EMAIL_SERVER, POP3_PORT_SERVER);
            smtpClient.Credentials = new NetworkCredential(EMAIL_ACCOUNT, pwd);
            try
            {
                MailMessage mailMessage;
                mailMessage = new MailMessage();
                // Attach file
                Attachment att = new Attachment(attachmentFile);
                mailMessage.Attachments.Add(att);
                mailMessage.Subject = subject;                
                mailMessage.From = new MailAddress(EMAIL_ACCOUNT, "Smart POS");                
                string[] mails = mailTo.Split(';');
                int sent = 0;
                foreach (string mail in mails)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(mail, EMAIL_FORMAT, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        // Add receiver
                        mailMessage.To.Add(mail);
                        mailMessage.Body = body.Replace("{UserName}", customerName);
                        if (sent % 5 == 0)
                        {
                            System.Threading.Thread.Sleep(2000);
                        }
                        // Send email to customer
                        smtpClient.Send(mailMessage);
                        sent++;
                        mailMessage.To.Clear();

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
            catch (System.IndexOutOfRangeException)
            {
                return errorList;
            }
        }
        #endregion

        #region xml heper
        /// <summary>
        /// Get Nam from key in xml file
        /// </summary>
        /// <param name="key">key to lookup</param>
        /// <param name="param">Collection to lookup</param>
        /// <returns></returns>
        private string ConvertXMLKeyToName(object obj, string param)
        {
            try
            {
                ComboItem cboItem = new ComboItem();
                if (obj != DBNull.Value)
                {
                    int key = int.Parse(obj.ToString());
                    switch (param)
                    {
                        // Get payment method by key
                        case "PaymentMethods":
                            cboItem = Common.PaymentMethods.SingleOrDefault(x => x.Value == key);
                            break;
                        // Get status by key
                        case "StatusBasic":
                            cboItem = Common.StatusBasic.SingleOrDefault(x => x.Value == key);
                            break;
                        // Get State by key
                        case "states":
                            cboItem = Common.States.SingleOrDefault(x => x.Value == key);
                            break;
                        // Get country by key
                        case "country":
                            cboItem = Common.Countries.SingleOrDefault(x => x.Value == key);
                            break;
                        // Get Title by key
                        case "Title":
                            cboItem = Common.Title.SingleOrDefault(x => x.Value == key);
                            break;
                        // Get Marital status by key
                        case "MaritalStatus":
                            cboItem = Common.MaritalStatus.SingleOrDefault(x => x.Value == key);
                            break;
                    }
                }
                if(cboItem != null)
                {
                    return cboItem.Text;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region -Date Time converter-
        /// <summary>
        /// Format date
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>formated date</returns>
        public static string ToShortDateString(object dt)
        {
            return (dt == DBNull.Value) ? "" : string.Format("{0:MM/dd/yyyy}", dt);
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

        #region -Get configuration-
               
        /// <summary>
        /// Get configuration
        /// </summary>
        private void GetConfiguration()
        {
            // Get company config
            da = dbHelper.ExecuteQuery("v_configuration");
            if (da.Rows.Count > 0)
            {
                // Add data to report header
                ds.Tables["Header"].Rows.Add(
                        da.Rows[0][0], da.Rows[0][1], da.Rows[0][2], 
                        PhoneNumberFormat(da.Rows[0][3].ToString()),
                        da.Rows[0][4], da.Rows[0][5], "Just do it"
                    );
                // Get Currency Symbol
                currencySymbol = "'" + da.Rows[0][8].ToString() +"'";
            }
        }
        #endregion

        #region -Load Sale Order report-
        /// <summary>
        /// Load Sale Order report
        /// </summary>
        private void LoadSalerOrderReport(long selectedSaleOrder)
        {            
            dtConfig = dbHelper.ExecuteQuery("v_configuration");
            dtSO = dbHelper.ExecuteQuery("sp_get_sale_order_by_id", selectedSaleOrder.ToString());
            ds.Tables["SOHeader"].Rows.Add(
                    dtConfig.Rows[0][0], dtConfig.Rows[0][1], dtConfig.Rows[0][2], 
                    PhoneNumberFormat(dtConfig.Rows[0][3].ToString()),
                    dtConfig.Rows[0][4], dtConfig.Rows[0][5], FaxFormat(dtConfig.Rows[0][6].ToString()),
                    dtSO.Rows[0][0], dtSO.Rows[0][1], dtSO.Rows[0][1], dtSO.Rows[0][3]
                );
            DataTable dtSaleOrderDetails = dbHelper.ExecuteQuery("sp_get_sale_order_details_by_id", selectedSaleOrder.ToString());
            int count = dtSaleOrderDetails.Rows.Count;
            if (count == 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No record");
                return;
            }
            int i = 0;
            for (i = 0; i < count; i++)
            {
                ds.Tables["SaleOrderDetails"].Rows.Add(
                     dtSaleOrderDetails.Rows[i][0], dtSaleOrderDetails.Rows[i][1], dtSaleOrderDetails.Rows[i][2],
                     dtSaleOrderDetails.Rows[i][3], dtSaleOrderDetails.Rows[i][4]
                    );
            }
            saleOrderReport = new rptSalesOrder();
            saleOrderReport.SetDataSource(ds);
            ReportSource = saleOrderReport;
        }
        #endregion

        #region -Get all Store-
        /// <summary>
        /// Get Get all Store
        /// </summary>
        private string GetStoreNameByStoreCode(int storeCode)
        {
            // Get all store name
            DataTable da = dbHelper.ExecuteQuery("v_get_all_store_name");
            if (da.Rows.Count > 0)
            {
                StoreModelCollection = new List<string>();
                for (int i = 0; i < da.Rows.Count; i++)
                {
                    // add store name to list
                    StoreModelCollection.Add(da.Rows[i][0].ToString());
                }
                return StoreModelCollection[storeCode];
            }
            return string.Empty;        
        }
        #endregion

        #region -Convert Image to byte Array-
        /// <summary>
        /// Convert Image to byte Array
        /// </summary>
        private void ConvertImageToByteArray()
        {
            try
            {
                // Convert true image to byte array
                string path = "CPC.POS.Image.Report.TrueFalseImgs.True.jpg";
                Stream stream = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream(path);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    stream.CopyTo(ms);
                    trueImg = ms.ToArray();
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion
        #endregion        
    }
}