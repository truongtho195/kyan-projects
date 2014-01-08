using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolkit.Base;
using CPC.POSReport.Repository;
using System.Collections.ObjectModel;
using CPC.POSReport.Model;
using CPC.POSReport.Function;
using CPC.POSReport.CrystalReport.Dataset;
using System.Windows.Input;
using Toolkit.Command;
using System.Drawing.Printing;
using System.IO;
using Microsoft.Win32;
using CPC.POSReport.View;
using System.Xml.Linq;
using System.Reflection;
using System.Data;
using CPC.POSReport.CrystalReport.Report.Inventory;
using CPC.POSReport.CrystalReport.Report.Sales;
using System.ComponentModel;
using POSReport;
using CPC.POSReport.CrystalReport.Report.Purchasing;
using Xceed.Wpf.Toolkit;
using CrystalDecisions.Shared;
using CPC.POSReport.View.PopupForm.Report.ADOPDF;

namespace CPC.POSReport.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        #region -Contructor-
        public MainViewModel()
        {
            if (Common.IS_LOG_OUT)
            {
                RefreshRepository();
                Common.IS_LOG_OUT = false;
            }
            InitCommand();
            // Load all Group report
            LoadGroupReport();
            // Get all parent by group
            if (GroupReportModel != null)
            {
                GetParentReport(GroupReportModel.Id);
            }
            if (PaperSizeCollection == null)
            {
                PaperSizeCollection = xmlHelper.GetAllPaperSize();
            }
            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.RunWorkerAsync();
        }

        private void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            if (ReportModelCollection != null && ReportModelCollection.Count > 0)
            {
                foreach (var item in ReportModelCollection)
                {
                    DeleteOldHistoryPrintedList(item.Code);
                }
            }
        }

        #endregion

        #region -Properties-

        #region -Set focus default-
        private bool _focusDefault;
        /// <summary>
        /// Gets or sets the FocusDefault.
        /// </summary>
        public bool FocusDefault
        {
            get { return _focusDefault; }
            set
            {
                if (_focusDefault != value)
                {
                    _focusDefault = value;
                    OnPropertyChanged(() => FocusDefault);
                }
            }
        }
        #endregion

        #region -Printed collection-

        private PrintedModel _printedModel;

        public PrintedModel PrintedModel
        {
            get { return _printedModel; }
            set
            {
                if (_printedModel != value)
                {
                    _printedModel = value;
                    OnPropertyChanged(() => PrintedModel);
                }
            }
        }
        private ObservableCollection<PrintedModel> _printedCollection = new ObservableCollection<PrintedModel>();

        public ObservableCollection<PrintedModel> PrintedCollection
        {
            get { return _printedCollection; }
            set
            {
                if (_printedCollection != value)
                {
                    _printedCollection = value;
                    OnPropertyChanged(() => PrintedCollection);
                }
            }
        }
        #endregion

        /// <summary>
        /// Get Login Name
        /// </summary>
        public string UserLoginName
        {
            get { return Common.LOGIN_NAME.ToLower(); }
        }
        /// <summary>
        /// Get computer name
        /// </summary>
        public string ComputerName
        {
            get { return Environment.MachineName; }
        }
        /// <summary>
        /// Get Database Name
        /// </summary>
        public string DatabaseName
        {
            get
            {
                Npgsql.NpgsqlConnectionStringBuilder connBuilder = new Npgsql.NpgsqlConnectionStringBuilder(
                        new System.Data.EntityClient.EntityConnectionStringBuilder(
                            System.Configuration.ConfigurationManager.ConnectionStrings["POSDBEntities"].ConnectionString)
                        .ProviderConnectionString);
                return connBuilder.Database;
            }
        }

        #region -Report Model Collection-
        /// <summary>
        /// Get or set Hidden Report model collection
        /// </summary>
        private ObservableCollection<rpt_ReportModel> _hiddenReportModelCollection;
        public ObservableCollection<rpt_ReportModel> HiddenReportModelCollection
        {
            get { return _hiddenReportModelCollection; }
            set
            {
                if (_hiddenReportModelCollection != value)
                {
                    _hiddenReportModelCollection = value;
                    OnPropertyChanged(() => HiddenReportModelCollection);
                }
            }
        }

        /// <summary>
        /// Get or set Report model collection
        /// </summary>
        private ObservableCollection<rpt_ReportModel> _reportModelCollection;
        public ObservableCollection<rpt_ReportModel> ReportModelCollection
        {
            get { return _reportModelCollection; }
            set
            {
                if (_reportModelCollection != value)
                {
                    _reportModelCollection = value;
                    OnPropertyChanged(() => ReportModelCollection);
                }
            }
        }

        /// <summary>
        /// Get or set Report model collection
        /// </summary>
        private ObservableCollection<rpt_ReportModel> _cCReportModelCollection = new ObservableCollection<rpt_ReportModel>();
        public ObservableCollection<rpt_ReportModel> CCReportModelCollection
        {
            get { return _cCReportModelCollection; }
            set
            {
                if (_cCReportModelCollection != value)
                {
                    _cCReportModelCollection = value;
                    OnPropertyChanged(() => CCReportModelCollection);
                }
            }
        }

        /// <summary>
        /// Get or set Report model collection
        /// </summary>
        private ObservableCollection<rpt_ReportModel> _allReportModelCollection;
        public ObservableCollection<rpt_ReportModel> AllReportModelCollection
        {
            get { return _allReportModelCollection; }
            set
            {
                if (_allReportModelCollection != value)
                {
                    _allReportModelCollection = value;
                    OnPropertyChanged(() => AllReportModelCollection);
                }
            }
        }

        /// <summary>
        /// Get or set Parent Report model collection
        /// </summary>
        private ObservableCollection<rpt_ReportModel> _parentReportModelCollection;
        public ObservableCollection<rpt_ReportModel> ParentReportModelCollection
        {
            get { return _parentReportModelCollection; }
            set
            {
                if (_parentReportModelCollection != value)
                {
                    _parentReportModelCollection = value;
                    OnPropertyChanged(() => ParentReportModelCollection);
                }
            }
        }

        /// <summary>
        /// Get or set Parent Report model collection
        /// </summary>
        private ObservableCollection<rpt_ReportModel> _currentParentReportModelCollection;
        public ObservableCollection<rpt_ReportModel> CurrentParentReportModelCollection
        {
            get { return _currentParentReportModelCollection; }
            set
            {
                if (_currentParentReportModelCollection != value)
                {
                    _currentParentReportModelCollection = value;
                    OnPropertyChanged(() => CurrentParentReportModelCollection);
                }
            }
        }

        /// <summary>
        /// Get or set Report model
        /// </summary>
        private rpt_ReportModel _reportModel;
        public rpt_ReportModel ReportModel
        {
            get { return _reportModel; }
            set
            {
                if (_reportModel != value)
                {
                    _reportModel = value;
                    OnPropertyChanged(() => ReportModel);
                }
            }
        }

        /// <summary>
        /// Get or set Hidden Report model
        /// </summary>
        private rpt_ReportModel _hiddenreportModel;
        public rpt_ReportModel HiddenReportModel
        {
            get { return _hiddenreportModel; }
            set
            {
                if (_hiddenreportModel != value)
                {
                    _hiddenreportModel = value;
                    OnPropertyChanged(() => HiddenReportModel);
                }
            }
        }

        #endregion

        #region -Group report collection-
        /// <summary>
        /// Get or set Group model collection
        /// </summary>
        private ObservableCollection<rpt_GroupModel> _groupReportModelCollection;
        public ObservableCollection<rpt_GroupModel> GroupReportModelCollection
        {
            get { return _groupReportModelCollection; }
            set
            {
                if (_groupReportModelCollection != value)
                {
                    _groupReportModelCollection = value;
                    OnPropertyChanged(() => GroupReportModelCollection);
                }
            }
        }
        #endregion

        #region -Group Report Model-
        /// <summary>
        /// Set or get RptGroup
        /// </summary>
        private rpt_GroupModel _groupReportModel;
        public rpt_GroupModel GroupReportModel
        {
            get { return _groupReportModel; }
            set
            {
                if (_groupReportModel != value)
                {
                    _groupReportModel = value;
                    OnPropertyChanged(() => GroupReportModel);
                }
            }
        }
        #endregion

        #region -Store model collection-
        /// <summary>
        /// Set or get Store model collection
        /// </summary>
        private ObservableCollection<base_StoreModel> _storeModelCollection;
        public ObservableCollection<base_StoreModel> StoreModelCollection
        {
            get { return _storeModelCollection; }
            set
            {
                if (_storeModelCollection != value)
                {
                    _storeModelCollection = value;
                    OnPropertyChanged(() => StoreModelCollection);
                }
            }
        }
        #endregion

        #region -Is Show Control-

        /// <summary>
        /// Set or get Is Show MainGrid
        /// </summary>
        private string _isShowMainGrid = "Visible";
        public string IsShowMainGrid
        {
            get { return _isShowMainGrid; }
            set
            {
                if (_isShowMainGrid != value)
                {
                    _isShowMainGrid = value;
                    OnPropertyChanged(() => IsShowMainGrid);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Hidden Report Property
        /// </summary>
        private string _isShowHiddenReport = "Collapsed";
        public string IsShowHiddenReport
        {
            get { return _isShowHiddenReport; }
            set
            {
                if (_isShowHiddenReport != value)
                {
                    _isShowHiddenReport = value;
                    OnPropertyChanged(() => IsShowHiddenReport);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Main Screen Property
        /// </summary>
        private string _isShowMainScreen = "Visible";
        public string IsShowMainScreen
        {
            get { return _isShowMainScreen; }
            set
            {
                if (_isShowMainScreen != value)
                {
                    _isShowMainScreen = value;
                    OnPropertyChanged(() => IsShowMainScreen);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Change Print Property
        /// </summary>
        private string _isShowChangePrintProperty = "Collapsed";
        public string IsShowChangePrintProperty
        {
            get { return _isShowChangePrintProperty; }
            set
            {
                if (_isShowChangePrintProperty != value)
                {
                    _isShowChangePrintProperty = value;
                    OnPropertyChanged(() => IsShowChangePrintProperty);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Print Property
        /// </summary>
        private string _isShowPrintProperty = "Collapsed";
        public string IsShowPrintProperty
        {
            get { return _isShowPrintProperty; }
            set
            {
                if (_isShowPrintProperty != value)
                {
                    _isShowPrintProperty = value;
                    OnPropertyChanged(() => IsShowPrintProperty);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Set Copy Property
        /// </summary>
        private string _isShowSetCopy = "Collapsed";
        public string IsShowSetCopy
        {
            get { return _isShowSetCopy; }
            set
            {
                if (_isShowSetCopy != value)
                {
                    _isShowSetCopy = value;
                    OnPropertyChanged(() => IsShowSetCopy);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Edit Report Property
        /// </summary>
        private string _isShowEditReport = "Collapsed";
        public string IsShowEditReport
        {
            get { return _isShowEditReport; }
            set
            {
                if (_isShowEditReport != value)
                {
                    _isShowEditReport = value;
                    OnPropertyChanged(() => IsShowEditReport);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Print Button Group Property
        /// </summary>
        private string _isShowPrintButtonGroup = "Collapsed";
        public string IsShowPrintButtonGroup
        {
            get { return _isShowPrintButtonGroup; }
            set
            {
                if (_isShowPrintButtonGroup != value)
                {
                    _isShowPrintButtonGroup = value;
                    OnPropertyChanged(() => IsShowPrintButtonGroup);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Print Property
        /// </summary>
        private string _isShowSampleReport = "Collapsed";
        public string IsShowSampleReport
        {
            get { return _isShowSampleReport; }
            set
            {
                if (_isShowSampleReport != value)
                {
                    _isShowSampleReport = value;
                    OnPropertyChanged(() => IsShowSampleReport);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Report Property
        /// </summary>
        private string _isShowReport;
        public string IsShowReport
        {
            get { return _isShowReport; }
            set
            {
                if (_isShowReport != value)
                {
                    _isShowReport = value;
                    OnPropertyChanged(() => IsShowReport);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Print Property
        /// </summary>
        private string _isShowImageReport;
        public string IsShowImageReport
        {
            get { return _isShowImageReport; }
            set
            {
                if (_isShowImageReport != value)
                {
                    _isShowImageReport = value;
                    OnPropertyChanged(() => IsShowImageReport);
                }
            }
        }

        /// <summary>
        /// Set or get Is Show Zoom icon Property
        /// </summary>
        private string _isShowZoomIcon = "Visible";
        public string IsShowZoomIcon
        {
            get { return _isShowZoomIcon; }
            set
            {
                if (_isShowZoomIcon != value)
                {
                    _isShowZoomIcon = value;
                    OnPropertyChanged(() => IsShowZoomIcon);
                }
            }
        }

        #endregion

        #region -Get all page size-
        /// <summary>
        /// Set or get page Size
        /// </summary>
        private IList<ComboItem> _paperSizeCollection;
        public IList<ComboItem> PaperSizeCollection
        {
            get { return _paperSizeCollection; }
            set
            {
                _paperSizeCollection = value;
            }
        }

        #endregion

        #region -Get all printer-
        /// <summary>
        /// Get all printer from local computer
        /// </summary>
        public PrinterSettings.StringCollection InstalledPrinters
        {
            get
            {
                return PrinterSettings.InstalledPrinters;
            }
        }
        #endregion

        #region -CurrentPrinter -
        /// <summary>
        /// Set or get CurrentPrinter
        /// </summary>
        private string _currentPrinter;
        public string CurrentPrinter
        {
            get { return _currentPrinter; }
            set
            {
                if (_currentPrinter != value)
                {
                    _currentPrinter = value;
                    OnPropertyChanged(() => CurrentPrinter);
                }
            }
        }
        #endregion

        #region -Report Source-
        private object _reportSource;
        public object ReportSource
        {
            get { return _reportSource; }
            set
            {
                if (_reportSource != value)
                {
                    _reportSource = value;
                    OnPropertyChanged(() => ReportSource);
                }
            }
        }
        #endregion

        #region -Is Check All Hidden Report-
        private object _isCheckAllHiddenReport;
        public object IsCheckAllHiddenReport
        {
            get { return _isCheckAllHiddenReport; }
            set
            {
                if (_isCheckAllHiddenReport != value)
                {
                    _isCheckAllHiddenReport = value;
                    OnPropertyChanged(() => IsCheckAllHiddenReport);
                }
            }
        }
        #endregion

        #endregion -Properties-

        #region -Defines-
        // Class XMLHelper: support get data from XML
        XMLHelper xmlHelper = new XMLHelper();
        // Get data from database to data table
        DBHelper dbHelp = new DBHelper();
        DataTable da = new DataTable();

        // Parameter in SQL function
        string param = string.Empty;
        string slectedStoreName = string.Empty;
        public short currentStoreCode = -1;
        public short transferTo = -1;
        public int departId = -1;
        public int categoryId = -1;
        public string customerResource = "''";
        public string productResource = "''";
        public string fromDate = string.Empty;
        public string toDate = string.Empty;
        public string shipFrom = string.Empty;
        public string shipTo = string.Empty;
        public int adjustmentStatus = -1;
        public int adjustmentReason = -1;
        public int saleOrderStatus = -1;
        public int transferStockStatus = -1;
        public int purchaseOrderStatus = -1;
        public int countryValue = -1;
        string exportFile = string.Empty;
        // true false image (report Card List)
        byte[] trueImg;

        Report report = new Report();
        ViewReportWindow viewReportWindow;
        // Popup Filter
        ReportOptional reportPopup;
        PurchaseOptional purchaseOptional;
        CustomerPaymentOptional customerPaymentOptional;
        // Inventory
        rptProductList productListReport;
        rptCostAdjustment costAdjustmentReport;
        rptQuantityAdjustment quantityAdjustmentReport;
        rptProductSummaryActivity productSummaryActivity;
        rptCategoryList categoryListReport;
        rptReOrderStock reOrderStockReport;
        rptTransferHistory transferHistoryReport;
        rptTransferHistoryDetails transferHistoryDetailsReport;
        // Purchasing
        rptPOSummary pOSummaryReport;
        rptPODetails pODetailsReport;
        rptProductCost productCostReport;
        rptVendorProductList vendorProductListReport;
        rptVendorList vendorListReport;
        // Sale Order
        rptSaleByProductSummary salebyProductSummaryReport;
        rptSaleByProductDetails salebyProductDetailsReport;
        rptSaleOrderSummary saleOrderSummaryReport;
        rptSaleProfitSummary saleProfitSummaryReport;
        rptSaleOrderOperational saleOrderOperationReport;
        rptCustomerPaymentSummary customerPaymentSummaryReport;
        rptCustomerPaymentDetails customerPaymentDetailsReport;
        rptProductCustomer productCustomerReport;
        rptCustomerOrderHistory customerOrderHistoryReport;
        rptSaleRepresentative saleRepresentativeReport;
        rptSaleCommission saleCommissionReport;
        rptSaleCommissionDetails saleCommissionDetailsReport;
        rptGiftCertificateList giftCertificateReport;
        rptVoidedInvoice voidedInvoiceReport;
        rptSOLocked sOLockedReport;
        rptPOLocked pOLockedReport;
        // Resository
        base_StoreRepository storeRepo = new base_StoreRepository();
        rpt_GroupRepository groupRepo = new rpt_GroupRepository();
        rpt_ReportRepository reportRepo = new rpt_ReportRepository();
        rpt_PermissionRepository permissionRepo = new rpt_PermissionRepository();
        public FilterReportViewModel filterReportViewModel { get; set; }
        // Default is preview report
        bool isPrint = false;

        private List<rpt_ReportModel> lstHiddenReport;
        private List<rpt_GroupModel> lstGroupReportModel;
        rpt_ReportModel ReportStoreModel;
        #endregion -Defines-

        #region -Command-

        #region -Init command-
        /// <summary>
        /// Init all command
        /// </summary>        
        private void InitCommand()
        {
            // Print command 
            ChangePrintPropertyCommand = new RelayCommand(ChangePrintPropertyExecute, CanChangePrintProperty);
            CancelChangePrintPropertyCommand = new RelayCommand(CancelChangePrintPropertyExecute);
            PrintPropertyCommand = new RelayCommand(PrintPropertyExecute, CanPrintPropertyExecute);
            PrintDirectCommand = new RelayCommand(PrintDirectExecute, CanPrintDirectExecute);
            ScreenCommand = new RelayCommand(ScreenExecute, CanScreenExecute);
            // Set copy
            SetCopyCommand = new RelayCommand(SetCopyExecute, CanSetCopyExecute);
            NewSetCopyCommand = new RelayCommand(NewSetCopyExecute, CanNewSetCopyExecute);
            DeleteSetCopyCommand = new RelayCommand(DeleteSetCopyExecute, CanDeleteSetCopyExecute);
            HideSetCopyCommand = new RelayCommand(HideSetCopyExecute);
            // Load treeview
            ChangeMajorGroupCommand = new RelayCommand<rpt_GroupModel>(ChangeMajorGroupExecute, CanChangeMajorGroupExecute);
            SelectedItemChanged = new RelayCommand<System.Windows.Controls.TreeView>(SelectedItemChangedExecute, CanSelectedItemChangedExecute);
            GotFocusCommand = new RelayCommand<System.Windows.Controls.TreeView>(GotFocusExecute, CanSelectedItemChangedExecute);
            // Manage Report
            NewReportCommand = new RelayCommand(NewReportExecute, CanNewReportExecute);
            SaveReportCommand = new RelayCommand(SaveReportExecute, CanSaveReportExecute);
            EditReportCommand = new RelayCommand(EditReportExecute, CanEditReportExecute);
            DeleteReportCommand = new RelayCommand(DeleteReportExecute, CanDeleteReportExecute);
            NoShowReportCommand = new RelayCommand(NoShowReportExecute, CanNoShowReportExecute);
            PreviewReportCommand = new RelayCommand(PreviewReportExecute, CanPreviewReportExecute);
            CloseReportCommand = new RelayCommand(CloseReportExecute);
            ReportFileCommand = new RelayCommand(ReportFileExecute);
            LoadSamplePictureReportCommand = new RelayCommand(LoadSamplePictureReportExecute, CanLoadSamplePictureReportExecute);
            ClearSamplePictureReportCommand = new RelayCommand(ClearSamplePictureReportExecute, CanClearSamplePictureReportExecute);
            ReportPermissionCommand = new RelayCommand(ReportPermissionExecute, CanReportPermissionExecute);
            ChangeGroupReportCommand = new RelayCommand(ChangeGroupExecute, CanChangeGroupExecute);
            ShowImageReportCommand = new RelayCommand(ShowImageReportExecute, CanShowImageReportExecute);
            ShowHiddenReportCommand = new RelayCommand(ShowHiddenReportExecute, CanShowHiddenReportExecute);
            DeleteHiddenReportCommand = new RelayCommand(DeleteHiddenReportExecute, CanDeleteHiddenReportExecute);
            CheckIsShowAllHiddenReportCommand = new RelayCommand<object>(ShowAllHiddenReportExecute, CanShowAllHiddenReportExecute);
            OpenPdfFileCommand = new RelayCommand(OpenPdfFileExecute, CanOpenPdfFileExecute);
            CheckIsShowHiddenReportCommand = new RelayCommand<object>(ShowHiddenReportExecute, CanShowHiddenReportExecute);
            // Menu command
            ChangePasswordCommand = new RelayCommand(ChangePasswordExecute, CanChangePasswordExecute);
            PermissionCommand = new RelayCommand(PermissionExecute, CanPermissionExecute);
            LogOutCommand = new RelayCommand(LogOutExecute);
            OpenShorcutViewCommand = new RelayCommand(OpenShorcutExecute);
        }
        #endregion

        #region -Print group command-
        #region -Change Print Property command-
        /// <summary>
        /// Set or get Change print property command
        /// </summary>
        public ICommand ChangePrintPropertyCommand { get; private set; }

        /// <summary>
        /// Update Report to database
        /// </summary>
        private void ChangePrintPropertyExecute()
        {
            try
            {
                SetVisibility(2);
                CurrentPrinter = CheckPrinter(ReportModel.PrinterName);
                if (!ReportModel.IsDefaultPrinter)
                {
                    ReportModel.PrinterName = ReportStoreModel.PrinterName;
                }
                ReportModel.ToEntity();
                reportRepo.Commit();
                BackupReport();
                ReportModel.PrinterName = CurrentPrinter;
                ReportModel.EndUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool CanChangePrintProperty()
        {
            if (ReportModel != null && ReportModel.Errors.Count() == 0 && ReportModel.IsDirty)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region -Cancel Change Print Property command-
        /// <summary>
        /// Set or get Cancel change print property command
        /// </summary>
        public ICommand CancelChangePrintPropertyCommand { get; private set; }

        private void CancelChangePrintPropertyExecute()
        {
            SetVisibility(2);
            if (ReportModel != null)
            {
                RestoreReport();
                ReportModel.EndUpdate();
            }
        }
        #endregion

        #region -Print Property command-
        /// <summary>
        /// Set or get Print property command
        /// </summary>
        public ICommand PrintPropertyCommand { get; private set; }

        private void PrintPropertyExecute()
        {
            SetVisibility(3);
        }

        public bool CanPrintPropertyExecute()
        {
            if (Common.SET_PRINT_COPY && ReportModel != null && ReportModel.ParentId != 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region -Print Direct Property command-
        /// <summary>
        /// Set or get Print direct property command
        /// </summary>
        public ICommand PrintDirectCommand { get; private set; }

        private void PrintDirectExecute()
        {
            SetVisibilityReport(true);
            isPrint = true;
            LoadReport();

        }

        private bool CanPrintDirectExecute()
        {
            if (ReportModel != null && ReportModel.Errors.Count() == 0 && !ReportModel.IsNew
                    && ReportModel.ParentId != 0 && !string.IsNullOrEmpty(ReportModel.FormatFile))
            {
                if (CheckPrintable())
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region -Screen command-
        /// <summary>
        /// Set or get Screen command
        /// </summary>
        public ICommand ScreenCommand { get; private set; }

        private void ScreenExecute()
        {
            SetVisibility(2);
            SetVisibilityReport(true);
            isPrint = false;
            // Load data to crystal report viewer
            LoadReport();
        }

        private bool CanScreenExecute()
        {
            if (ReportModel != null && ReportModel.Errors.Count() == 0 && !ReportModel.IsNew
                    && ReportModel.ParentId != 0 && !string.IsNullOrEmpty(ReportModel.FormatFile))
            {
                if (CheckViewable())
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #endregion

        #region -Set copy Group command-
        #region -Set copies command-
        /// <summary>
        /// Set or get Set Copy command
        /// </summary>
        public ICommand SetCopyCommand { get; private set; }

        private void SetCopyExecute()
        {
            SetVisibility(1);
        }
        private bool CanSetCopyExecute()
        {
            return (Common.VIEW_SET_COPY && ReportModel != null && ReportModel.ParentId != 0);
        }
        #endregion

        #region -New Set copies command-
        /// <summary>
        /// Set or get New Set copies command
        /// </summary>
        public ICommand NewSetCopyCommand { get; private set; }

        private void NewSetCopyExecute()
        {
            CCToView ccTo = new CCToView();
            ccTo.DataContext = new CCToViewModel(ccTo, ReportModel.CCReport, this);
            ccTo.ShowDialog();
        }

        private bool CanNewSetCopyExecute()
        {
            return (ReportModel != null && Common.NEW_SET_COPY);
        }
        #endregion

        #region -Delete Set copies command-
        /// <summary>
        /// Set or get New Set copies command
        /// </summary>
        public ICommand DeleteSetCopyCommand { get; private set; }

        private void DeleteSetCopyExecute()
        {
            System.Windows.MessageBoxResult resuilt = MessageBox.Show(
                                    "Do you want to delete all email(s)?\n" + ReportModel.CCReport, "Warning",
                                    System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning
                                );
            if (System.Windows.MessageBoxResult.Yes.Equals(resuilt))
            {
                try
                {
                    ReportModel.CCReport = string.Empty;
                    ReportModel.ToEntity();
                    reportRepo.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        private bool CanDeleteSetCopyExecute()
        {
            return (ReportModel != null && !string.IsNullOrWhiteSpace(ReportModel.CCReport) && Common.DELETE_SET_COPY);
        }
        #endregion

        #region -Hide Set copies command-
        /// <summary>
        /// Set or get Hide Set Copy command
        /// </summary>
        public ICommand HideSetCopyCommand { get; private set; }

        private void HideSetCopyExecute()
        {
            SetVisibility(2);
        }
        #endregion

        #endregion

        #region -Load Treeview and Selected item group command-

        #region -Change Major Group Command command-
        /// <summary>
        /// Set or get Tabcontrol Selection Changed Command
        /// </summary>
        public ICommand ChangeMajorGroupCommand { get; private set; }

        /// <summary>
        /// Reload treeview when selected TabItem
        /// </summary>
        /// <param name="rptGroup"></param>
        private void ChangeMajorGroupExecute(rpt_GroupModel rptGroup)
        {
            if (ReportModel != null)
            {
                if (ReportModel.IsNew)
                {
                    ReportModel = null;
                }
            }
            // Get all parent report
            GetParentReport(rptGroup.Id);
            // Show Main grid if have no parent report
            if (ParentReportModelCollection.Count <= 1)
            {
                // Show Main grid
                SetIsShowHiddenReport(false);
                return;
            }
            // Load treeview by group id
            LoadTreeView(rptGroup.Id);
        }

        private bool CanChangeMajorGroupExecute(rpt_GroupModel rptGroup)
        {
            return (rptGroup != null);
        }
        #endregion

        #region -SelectedItem Changed Command-
        /// <summary>
        /// Get or set SelectedItemChanged Command
        /// </summary>
        public ICommand SelectedItemChanged { get; private set; }

        /// <summary>
        /// Reset Report model
        /// </summary>
        /// <param name="rptGroup"></param>
        private void SelectedItemChangedExecute(System.Windows.Controls.TreeView treeView)
        {
            if (ReportModel != null && ReportModel.IsDirty)
            {
                if (ReportModel.Errors.Count == 0 && !Common.IS_CHANGING_GROUP)
                {
                    System.Windows.MessageBoxResult resuilt = MessageBox.Show(
                                    "Do you want to save changes?", "Warning",
                                    System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning
                                );
                    if (System.Windows.MessageBoxResult.Yes.Equals(resuilt))
                    {
                        SaveReportExecute();
                    }
                }
                if (!ReportModel.IsNew)
                {
                    RestoreReport();
                }
            }
            // Reset old icon
            if (ReportModel != null)
            {
                //ParentReportModelCollection = CurrentParentReportModelCollection;
                if (ReportModel.Parent != null)
                {
                    // Show document Icon
                    SetTreeViewIcon(3);
                }
                else
                {
                    if (ReportModel.Children != null && ReportModel.Children.Count != 0)
                    {
                        // Show close folder icon
                        SetTreeViewIcon(2);
                    }
                    else
                    {
                        // Show open folder icon
                        SetTreeViewIcon(1);
                    }
                }
                ParentReportModelCollection = CurrentParentReportModelCollection;
            }
            if (CCReportModelCollection.Count > 0)
            {
                CCReportModelCollection.RemoveAt(0);
            }
            ReportModel = treeView.SelectedItem as rpt_ReportModel;
            if (ReportModel != null)
            {
                if (ReportModel.Parent == null)
                {
                    ParentReportModelCollection = new ObservableCollection<rpt_ReportModel>(ParentReportModelCollection.Where(x => x.Id == 0));
                    ShowHiddenReportList(ReportModel.Id);
                }
                else
                {
                    ParentReportModelCollection = new ObservableCollection<rpt_ReportModel>(CurrentParentReportModelCollection.Where(x => x.Id != 0));
                    SetVisibility(Common.PREVIOUS_SCREEN);
                }
                CurrentPrinter = CheckPrinter(ReportModel.PrinterName);
                GetPaperName();
                ReportModel.EndUpdate();
                // Set selected icon
                SetTreeViewIcon(4);
                BackupReport();
                CCReportModelCollection.Add(ReportModel);
            }
        }

        private bool CanSelectedItemChangedExecute(System.Windows.Controls.TreeView treeView)
        {
            return (treeView.SelectedItem != null);
        }

        /// <summary>
        /// Show hidden report in each mirror group
        /// </summary>
        /// <param name="reportId">Mirror Report Id</param>
        private void ShowHiddenReportList(int reportId)
        {
            // Show hidden report list
            HiddenReportModelCollection = new ObservableCollection<rpt_ReportModel>(
                        lstHiddenReport.FindAll(w => w.ParentId == reportId && !w.IsShow)
                    );
            SetVisibility(-1);
            // Set is show hidden report
            if (HiddenReportModelCollection.Count > 0)
            {
                SetIsShowHiddenReport(true);
                IsCheckAllHiddenReport = false;
            }
            else
            {
                // Show Main grid
                SetIsShowHiddenReport(false);
            }
        }
        #endregion

        #region -Got Focus Command-
        /// <summary>
        /// Get or set Got Focus Command
        /// </summary>
        public ICommand GotFocusCommand { get; private set; }

        /// <summary>
        /// Reset Report model
        /// </summary>
        /// <param name="rptGroup"></param>
        private void GotFocusExecute(System.Windows.Controls.TreeView treeView)
        {
            // Restore or save changes (or save new)
            if (ReportModel != null && ReportModel.IsDirty)
            {
                if (ReportModel.Errors.Count == 0 && !Common.IS_CHANGING_GROUP)
                {
                    System.Windows.MessageBoxResult resuilt = MessageBox.Show(
                                    "Do you want to save changes?", "Warning",
                                    System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning
                                );
                    if (System.Windows.MessageBoxResult.Yes.Equals(resuilt))
                    {
                        // Save changes or save new Report
                        SaveReportExecute();
                    }
                }
                if (ReportModel != null && !ReportModel.IsNew)
                {
                    RestoreReport();
                }
            }
            // Reset old icon
            if (ReportModel != null)
            {
                if (ReportModel.Parent != null)
                {
                    // Show document Icon
                    SetTreeViewIcon(3);
                }
                else
                {
                    if (ReportModel.Children != null && ReportModel.Children.Count != 0)
                    {
                        // Show close folder icon
                        SetTreeViewIcon(2);
                    }
                    else
                    {
                        // Show open folder icon
                        SetTreeViewIcon(1);
                    }
                }
            }
            ReportModel = treeView.SelectedItem as rpt_ReportModel;
            if (ReportModel != null)
            {
                // Set selected icon
                SetTreeViewIcon(4);
                if (IsShowPrintProperty == "Visible")
                {
                    SetVisibilityReport(false);
                }
                // Get Printed History List
                GetPrindHistoryList();
            }
        }

        #endregion
        #endregion

        #region -Manage Report group command-

        #region -New report command-
        /// <summary>
        /// Set or get New Report command
        /// </summary>
        public ICommand NewReportCommand { get; private set; }

        private void NewReportExecute()
        {
            int parentId = 0;
            if (ReportModel != null)
            {
                if (ReportModel.Parent != null && ReportModel.ParentId != 0)
                {
                    parentId = ReportModel.ParentId;
                    // Set document icon
                    SetTreeViewIcon(3);
                }
                else
                {
                    if (ReportModel.Children != null && ReportModel.Children.Count != 0)
                    {
                        // set Open icon
                        SetTreeViewIcon(1);
                    }
                    else
                    {
                        // Set close icon
                        SetTreeViewIcon(2);
                    }
                }
            }
            ReportModel = new rpt_ReportModel();
            ReportModel.ParentId = parentId;
            ReportModel.PaperSize = 9;
            // Show edit form
            SetVisibility(0);
            // Reset all parent report
            ParentReportModelCollection = CurrentParentReportModelCollection;
            FocusDefault = false;
            FocusDefault = true;
        }
        private bool CanNewReportExecute()
        {
            return Common.ADD_REPORT;
        }
        #endregion

        #region -Save report command-
        /// <summary>
        /// Set or get Save Report command
        /// </summary>
        public ICommand SaveReportCommand { get; private set; }

        /// <summary>
        /// Create new report or update
        /// </summary>
        private void SaveReportExecute()
        {
            try
            {
                int groupID = GetGroupReportPosition(GroupReportModel.Id);
                if (ReportModel.IsNew || (ReportModel.IsDirty && ReportStoreModel.Code != ReportModel.Code))
                {
                    if (CheckDuplicateReportCode(ReportModel.Code))
                    {
                        MessageBox.Show("Report code is duplicate.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }
                }
                if (ReportModel.IsNew)
                {
                    SaveReport(groupID);
                }
                else if (ReportModel.IsDirty)
                {
                    UpdateReport(groupID);
                }

                ReportModel.EndUpdate();
                if (ReportModel.ParentId == 0)
                {
                    // Show close icon
                    SetTreeViewIcon(2);
                }
                else
                {
                    // Show document icon
                    SetTreeViewIcon(3);
                }
                // Get current printer
                CurrentPrinter = CheckPrinter(ReportModel.PrinterName);
                // Get paper name
                GetPaperName();
                // Store Report model
                BackupReport();
                ReportModel = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Check can save report execute
        /// </summary>
        /// <returns></returns>
        private bool CanSaveReportExecute()
        {
            return (ReportModel != null && ReportModel.Errors.Count() == 0 && ReportModel.IsDirty);
        }
        #endregion

        #region -Edit report command-
        /// <summary>
        /// Set or get Edit Report command
        /// </summary>
        public ICommand EditReportCommand { get; private set; }

        private void EditReportExecute()
        {
            SetVisibility(0);
        }
        private bool CanEditReportExecute()
        {
            return (Common.EDIT_REPORT && ReportModel != null && !ReportModel.IsNew
                && CurrentParentReportModelCollection.Count > 1 && ReportModel.GroupId == GroupReportModel.Id);
        }
        #endregion

        #region -Delete report command-
        /// <summary>
        /// Set or get Delete Report command
        /// </summary>
        public ICommand DeleteReportCommand { get; private set; }
        /// <summary>
        /// Delete report execute
        /// </summary>
        private void DeleteReportExecute()
        {
            System.Windows.Forms.DialogResult dialog = (System.Windows.Forms.DialogResult)MessageBox.Show("Do you really want to delete?", "Warning", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
            if (System.Windows.Forms.DialogResult.Yes.Equals(dialog))
            {
                try
                {
                    //int reportID = ReportModel.Id;
                    int parentID = ReportModel.ParentId;
                    // Delete Report from database
                    reportRepo.Delete(ReportModel.rpt_Report);
                    reportRepo.Commit();
                    int groupID = GetGroupReportPosition(GroupReportModel.Id);
                    // Clear Tree node
                    if (ReportModel.Parent != null)
                    {
                        rpt_ReportModel parent = GroupReportModelCollection[groupID].RootReportColection.FirstOrDefault(x => x.Id == ReportModel.ParentId);
                        if (parent != null)
                        {
                            parent.Children.Remove(ReportModel);
                            //ReportModel.Parent.Children.Remove(ReportModel);
                        }
                    }
                    else
                    {
                        // set parent Report
                        ParentReportModelCollection.Remove(ReportModel);
                        CurrentParentReportModelCollection.Remove(ReportModel);
                        GroupReportModelCollection[groupID].RootReportColection.Remove(ReportModel);
                        ReportModelCollection.Remove(ReportModel);
                    }
                    ReportModel = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Check can delete report execute
        /// </summary>
        /// <returns></returns>
        private bool CanDeleteReportExecute()
        {
            if (Common.DELETE_REPORT && ReportModel != null && !ReportModel.IsNew)
            {
                if (ReportModel.Children == null || ReportModel.Children.Count == 0)
                {
                    if (ReportModel.ParentId == 0)
                    {
                        var rpt = AllReportModelCollection.Where(w => w.ParentId == ReportModel.Id);
                        return (rpt == null);
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region -No Show report command-
        /// <summary>
        /// Set or get No Show Report command
        /// </summary>
        public ICommand NoShowReportCommand { get; private set; }

        private void NoShowReportExecute()
        {
            System.Windows.MessageBoxResult dialog = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to no show this report?", "Warning", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
            if (dialog == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    int parentID = ReportModel.ParentId;
                    // Set IsShow Report is false
                    ReportModel.IsShow = false;

                    lstHiddenReport.Add(ReportModel);
                    ReportModel.ToEntity();
                    reportRepo.Commit();
                    ReportModel.EndUpdate();
                    int groupID = GetGroupReportPosition(GroupReportModel.Id);
                    // Clear Tree node
                    if (ReportModel.Parent != null)
                    {
                        rpt_ReportModel parent = GroupReportModelCollection[groupID].RootReportColection.FirstOrDefault(x => x.Id == ReportModel.ParentId);
                        if (parent != null)
                        {
                            parent.Children.Remove(ReportModel);
                        }
                    }
                    else
                    {
                        GroupReportModelCollection[groupID].RootReportColection.Remove(ReportModel);
                    }
                    ReportModel = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        private bool CanNoShowReportExecute()
        {
            if (Common.NO_SHOW_REPORT && ReportModel != null && !ReportModel.IsNew)
            {
                return (ReportModel.Children == null || ReportModel.Children.Count == 0);
            }
            return false;
        }
        #endregion

        #region -Preview report command-
        /// <summary>
        /// Set or get Preview Report command
        /// </summary>
        public ICommand PreviewReportCommand { get; private set; }

        private void PreviewReportExecute()
        {
            isPrint = false;
            LoadReport();
        }
        private bool CanPreviewReportExecute()
        {
            if (ReportModel != null && ReportModel.Errors.Count() == 0 && !ReportModel.IsNew
                && !ReportModel.IsDirty && ReportModel.ParentId != 0 && !string.IsNullOrEmpty(ReportModel.FormatFile))
            {
                return (CheckViewable());
            }
            return false;
        }
        #endregion

        #region -Close report command-
        /// <summary>
        /// Set or get New Report command
        /// </summary>
        public ICommand CloseReportCommand { get; private set; }

        private void CloseReportExecute()
        {
            SetVisibility(2);
            SetVisibilityReport(false);
        }
        #endregion

        #region -Report File command-
        /// <summary>
        /// Set or get Report file command
        /// </summary>
        public ICommand ReportFileCommand { get; private set; }

        private void ReportFileExecute()
        {
            Microsoft.Win32.FileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Choose report file (*.rpt)|*.rpt";
            if (dialog.ShowDialog() == true)
            {
                string fileName = string.Empty;
                int index = dialog.FileName.LastIndexOf('\\');
                if (index != 0)
                {
                    fileName = dialog.FileName.Substring(index + 1);
                    ReportModel.FormatFile = fileName.Substring(0, fileName.Length - 4);
                }
            }
        }
        #endregion

        #region -Report Permission Command-
        public ICommand ReportPermissionCommand { get; private set; }

        private void ReportPermissionExecute()
        {
            ReportPermissionView assignAuthorizeReport = new ReportPermissionView();
            assignAuthorizeReport.ShowDialog(assignAuthorizeReport, ReportModel.Code);
            // Update current user's right 
            if (Common.IS_RIGHT_CHANGE)
            {
                Common.IS_RIGHT_CHANGE = false;
                ReportModel.IsView = Common.IS_VIEW;
                ReportModel.IsPrint = Common.IS_PRINT;
                ReportModel.EndUpdate();
            }
        }
        private bool CanReportPermissionExecute()
        {
            return (ReportModel != null && ReportModel.Parent != null) ? Common.SET_ASSIGN_AUTHORIZE_REPORT : false;
        }
        #endregion

        #region -Change Group report command-
        /// <summary>
        /// Set or get Change Group Report command
        /// </summary>
        public ICommand ChangeGroupReportCommand { get; private set; }

        private void ChangeGroupExecute()
        {
            ChangeGroupView changeGroupView = new ChangeGroupView();
            changeGroupView.DataContext = new ChangeGroupViewModel(this, changeGroupView, ReportModel);
            changeGroupView.ShowDialog();
        }
        private bool CanChangeGroupExecute()
        {
            if (Common.CHANGE_GROUP_REPORT && ReportModel != null && ReportModel.Errors.Count() == 0 && !ReportModel.IsNew
                    && !ReportModel.IsDirty && ReportModel.ParentId != 0 && !string.IsNullOrEmpty(ReportModel.FormatFile))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region -Show Hidden Report -
        public ICommand ShowHiddenReportCommand { get; set; }

        private void ShowHiddenReportExecute()
        {
            try
            {
                int groupID = GetGroupReportPosition(GroupReportModel.Id);
                int reportId = -1;
                reportRepo.BeginTransaction();
                int count = HiddenReportModelCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    if (HiddenReportModelCollection[i].IsShow)
                    {
                        HiddenReportModelCollection[i].ToEntity();
                        lstHiddenReport.Remove(HiddenReportModelCollection[i]);
                        var parent = GroupReportModelCollection[groupID].RootReportColection.FirstOrDefault(x => x.Id == HiddenReportModelCollection[i].ParentId);
                        if (parent != null)
                        {
                            // Get parent report
                            HiddenReportModelCollection[i].Parent = parent;
                            parent.Children.Add(HiddenReportModelCollection[i]);
                            reportId = parent.Id;
                            // Show document Icon
                            HiddenReportModelCollection[i].IsShowOpenFolder = "Collapsed";
                            HiddenReportModelCollection[i].IsShowCloseFolder = "Collapsed";
                            HiddenReportModelCollection[i].IsShowOK = "Collapsed";
                            HiddenReportModelCollection[i].IsShowDocument = "Visible";
                        }
                    }
                }
                reportRepo.Commit();
                reportRepo.CommitTransaction();
                ShowHiddenReportList(reportId);
            }
            catch (Exception ex)
            {
                reportRepo.RollbackTransaction();
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanShowHiddenReportExecute()
        {
            if (HiddenReportModel != null)
            {
                int count = HiddenReportModelCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    if (HiddenReportModelCollection[i].IsShow)
                    {
                        return true;
                    }
                }
            }
            return false;

        }
        #endregion

        #region -Delete Hidden Report -
        public ICommand DeleteHiddenReportCommand { get; set; }

        private void DeleteHiddenReportExecute()
        {
            System.Windows.MessageBoxResult resuilt = Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want to delete?", "Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
            if (resuilt.Equals(System.Windows.MessageBoxResult.Yes))
            {
                try
                {
                    // Delete Report from database
                    reportRepo.Delete(HiddenReportModel.rpt_Report);
                    reportRepo.Commit();
                    HiddenReportModelCollection.Remove(HiddenReportModel);
                    lstHiddenReport.Remove(HiddenReportModel);
                    HiddenReportModel = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteHiddenReportExecute()
        {
            return (HiddenReportModel != null && Common.DELETE_REPORT);
        }
        #endregion

        #region -Check Is Show Hidden Report -
        public ICommand CheckIsShowHiddenReportCommand { get; set; }

        private void ShowHiddenReportExecute(object obj)
        {
            System.Windows.Controls.CheckBox chk = obj as System.Windows.Controls.CheckBox;
            HiddenReportModel.IsShow = chk.IsChecked.Value;
            IsCheckAllHiddenReport = true;
            int count = HiddenReportModelCollection.Count;
            for (int i = 0; i < count; i++)
            {
                if (!HiddenReportModelCollection[i].IsShow)
                {
                    IsCheckAllHiddenReport = false;
                    break;
                }
            }
        }

        private bool CanShowHiddenReportExecute(object obj)
        {
            return (obj != null);
        }
        #endregion

        #region -Check Is Show All Hidden Report -
        public ICommand CheckIsShowAllHiddenReportCommand { get; set; }

        private void ShowAllHiddenReportExecute(object obj)
        {
            System.Windows.Controls.CheckBox chk = obj as System.Windows.Controls.CheckBox;
            int count = HiddenReportModelCollection.Count;
            for (int i = 0; i < count; i++)
            {
                HiddenReportModelCollection[i].IsShow = chk.IsChecked.Value;
            }
        }

        private bool CanShowAllHiddenReportExecute(object obj)
        {
            return (obj != null);
        }
        #endregion

        #region -Open Pdf File Command -
        public ICommand OpenPdfFileCommand { get; set; }

        private void OpenPdfFileExecute()
        {
            //string agu = Directory.GetCurrentDirectory() + "\\PDF File\\MoonPdf\\MoonPdf.exe";
            //System.Diagnostics.Process.Start(agu);
            // PDFView pdfView = new PDFView(PrintedModel.FilePath);     
            try
            {
                if (PrintedModel != null)
                {
                    System.Diagnostics.Process.Start(PrintedModel.FilePath);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private bool CanOpenPdfFileExecute()
        {
            return true;
        }
        #endregion

        #endregion

        #region -Support group command-

        #region -Clear sample picture report command-
        /// <summary>
        /// Set or get Clear sample picture report command
        /// </summary>
        public ICommand ClearSamplePictureReportCommand { get; private set; }

        private void ClearSamplePictureReportExecute()
        {
            if (ReportModel.SamplePicture != null)
            {
                ReportModel.SamplePicture = null;
            }
        }

        private bool CanClearSamplePictureReportExecute()
        {
            return (ReportModel != null && ReportModel.SamplePicture != null);
        }
        #endregion

        #region -Load sample picture report command-
        /// <summary>
        /// Set or get Load sample picture report command
        /// </summary>
        public ICommand LoadSamplePictureReportCommand { get; private set; }

        private void LoadSamplePictureReportExecute()
        {
            Microsoft.Win32.FileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Choose image file(*.jpeg;*.jpg;*.png;*.bmp;*.gif)|*.jpeg;*jpg;*.png;*bmp;*gif";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    FileStream fs = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read);
                    byte[] imageByte = new byte[fs.Length];
                    fs.Read(imageByte, 0, System.Convert.ToInt32(fs.Length));
                    fs.Close();
                    ReportModel.SamplePicture = imageByte;
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access to the path \"" + dialog.FileName + "\" is denied.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private bool CanLoadSamplePictureReportExecute()
        {
            return (ReportModel != null);
        }
        #endregion

        #region -Show Image Report Command-
        //-Show Image Report Command-
        public ICommand ShowImageReportCommand { get; private set; }

        private void ShowImageReportExecute()
        {
            ViewReportWindow window = new ViewReportWindow();
            window.Show();
            window.DataContext = new ReportViewViewModel(ReportModel.SamplePicture, ReportModel.Name, window);
        }

        private bool CanShowImageReportExecute()
        {
            return (ReportModel != null && ReportModel.Errors.Count == 0 && !ReportModel.IsNew && !ReportModel.IsDirty && ReportModel.SamplePicture != null);
        }
        #endregion

        #endregion

        #region -Menu group command-

        #region -Change pwd command-

        //Add User Command-
        public ICommand ChangePasswordCommand { get; private set; }

        private void ChangePasswordExecute()
        {
            View.ChangePasswordView changPwdView = new ChangePasswordView();
            changPwdView.ShowDialog(changPwdView);
        }

        private bool CanChangePasswordExecute()
        {
            return (!Common.IS_ADMIN);
        }
        #endregion

        #region -Permission-

        /// <summary>
        /// Permission command
        /// </summary>
        public RelayCommand PermissionCommand { get; set; }

        public void PermissionExecute()
        {
            PermissionView permissionView = new PermissionView();
            permissionView.ShowDialog(permissionView);
            // View major group
            if (Common.IS_CHANGE_MAJOR_GROUP)
            {
                LoadGroupReport();
                if (!Common.IS_ADMIN)
                {
                    CheckPermission(-2);
                }
                Common.IS_CHANGE_MAJOR_GROUP = false;
            }
        }

        public bool CanPermissionExecute()
        {
            return Common.SET_PERMISSION;
        }
        #endregion

        #region -Log Out Command-

        //-Log Out Command-
        public ICommand LogOutCommand { get; private set; }

        private void LogOutExecute()
        {
            View.LoginView loginWindow = new View.LoginView();
            Common.IS_LOG_OUT = true;
            App.Current.MainWindow.Close();
            App.Current.MainWindow = loginWindow;
            loginWindow.Show();
        }
        #endregion

        #region -Log Out Command-

        //-Log Out Command-
        public ICommand OpenShorcutViewCommand { get; private set; }

        private void OpenShorcutExecute()
        {
            View.PopupForm.Report.ShorcutKeys.ShortcutKeysView shortcutView = new View.PopupForm.Report.ShorcutKeys.ShortcutKeysView();
            shortcutView.Owner = App.Current.MainWindow;
            shortcutView.ShowInTaskbar = false;
            shortcutView.ShowDialog();

        }
        #endregion

        #endregion

        #endregion -Command-

        #region -Load report-
        /// <summary>
        /// Load product report
        /// </summary>
        private void LoadReport()
        {
            viewReportWindow = new ViewReportWindow();
            report.Tables[Common.DT_HEADER].Clear();
            ReportSource = null;
            bool isLandscape = false;
            try
            {
                switch (ReportModel.FormatFile)
                {
                    #region -Inventory-
                    case Common.RPT_PRODUCT_LIST:
                        GetProductList();
                        isLandscape = true;
                        break;
                    case Common.RPT_COST_ADJUSTMENT:
                        GetCostAdjustment();
                        isLandscape = true;
                        break;
                    case Common.RPT_QTY_ADJUSTMENT:
                        GetQuantityAdjustment();
                        isLandscape = true;
                        break;
                    case Common.RPT_PRODUCT_SUMMARY_ACTIVITY:
                        GetProductSummaryActitivity();
                        break;
                    case Common.RPT_CATEGORY_LIST:
                        GetCategoryList();
                        break;
                    case Common.RPT_REORDER_STOCK:
                        GetReOrderStock();
                        isLandscape = true;
                        break;
                    case Common.RPT_TRANSFER_HISTORY:
                        GetTransferHistory();
                        isLandscape = true;
                        break;
                    case Common.RPT_TRANSER_DETAILS:
                        GetTransferHistoryDetails();
                        isLandscape = true;
                        break;
                    #endregion

                    #region -Purchasing-
                    case Common.RPT_PO_SUMMARY:
                        GetPOSummary();
                        isLandscape = true;
                        break;
                    case Common.RPT_PO_DETAILS:
                        GetPODetails();
                        break;
                    case Common.RPT_PRODUCT_COST:
                        GetProductCost();
                        break;
                    case Common.RPT_VENDOR_PRODUCT_LIST:
                        GetVendorProductList();
                        break;
                    case Common.RPT_VENDOR_LIST:
                        GetVendorList();
                        break;
                    case Common.RPT_PO_LOCKED:
                        GetPOLocked();
                        break;
                    #endregion

                    #region -Sales-
                    case Common.RPT_SALE_BY_PRODUCT_SUMMARY:
                        GetSaleByProductSummary();
                        isLandscape = true;
                        break;
                    case Common.RPT_SALE_BY_PRODUCT_DETAILS:
                        GetSaleByProductDetails();
                        isLandscape = true;
                        break;
                    case Common.RPT_SALE_ORDER_SUMMARY:
                        GetSaleOrderSummary();
                        isLandscape = true;
                        break;
                    case Common.RPT_SALE_PROFIT_SUMMARY:
                        GetSaleProfitSummary();
                        break;
                    case Common.RPT_SALE_ORDER_OPERATION:
                        GetSaleOrderOperational();
                        break;
                    case Common.RPT_CUSTOMER_PAYMENT_SUMMARY:
                        GetCustomerPaymentSummary();
                        break;
                    case Common.RPT_CUSTOMER_PAYMENT_DETAILS:
                        GetCustomerPaymentDetails();
                        isLandscape = true;
                        break;
                    case Common.RPT_PRODUCT_CUSTOMER:
                        GetProductCustomer();
                        break;
                    case Common.RPT_CUSTOMER_ORDER_HISTORY:
                        GetCustomerOrderHistory();
                        break;
                    case Common.RPT_SALE_REPRESENTATIVE:
                        GetSaleRepresentative();
                        break;
                    case Common.RPT_SALE_COMMISSION:
                        GetSaleCommission();
                        break;
                    case Common.RPT_SALE_COMMISSION_DETAILS:
                        GetSaleCommissionDetails();
                        break;
                    case Common.RPT_GIFT_CERTIFICATE_LIST:
                        GetGiftCertificateList();
                        isLandscape = true;
                        break;
                    case Common.RPT_VOIDED_INVOICE:
                        GetVoidedInvoice();
                        break;
                    case Common.RPT_SO_LOCKED:
                        GetSOLocked();
                        break;
                    #endregion

                    default:
                        MessageBox.Show("Report not found!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        break;
                }
                if (ReportSource != null)
                {
                    if (!isPrint)
                    {
                        // Show report window                        
                        viewReportWindow.DataContext = new ReportViewViewModel(ReportSource, ReportModel, viewReportWindow, CurrentPrinter);
                        // Update Screen time
                        ReportModel.ScreenTimes++;
                    }
                    else
                    {
                        // ReportSource 
                        CrystalDecisions.CrystalReports.Engine.ReportDocument obj = ReportSource as CrystalDecisions.CrystalReports.Engine.ReportDocument;
                        PrintToPrinter(obj, isLandscape);
                        ExportFile(obj);
                        // Get Printed History List
                        GetPrindHistoryList();
                        ReportModel.PrintTimes++;
                    }
                    ReportModel.ToEntity();
                    // Save changes
                    reportRepo.Commit();
                    ReportModel.ToModel();
                    ReportModel.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                if ((int)ex.TargetSite.MethodHandle.Value != 2065652676)
                {
                    MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                viewReportWindow.btnClose.IsEnabled = true;
                return;
            }
        }

        #region -Popup-

        /// <summary>
        /// Get configuration
        /// </summary>
        private void GetConfiguration()
        {
            // Get company config
            da = dbHelp.ExecuteQuery("v_configuration");
            if (da.Rows.Count > 0)
            {
                // Add data to report header
                report.Tables["Header"].Rows.Add(
                        da.Rows[0][0], da.Rows[0][1], da.Rows[0][2],
                        Common.PhoneNumberFormat(da.Rows[0][3].ToString()),
                        da.Rows[0][4], da.Rows[0][5], "Just do it"
                    );
                // Get default language
                Common.CURRENT_LANGUAGE = da.Rows[0][7].ToString();
                // Get Currency Symbol
                Common.CURRENT_SYMBOL = da.Rows[0][8].ToString();
                // Get Decimal places
                if (da.Rows[0][6] != DBNull.Value)
                {
                    short.TryParse(da.Rows[0][9].ToString(), out Common.DECIMAL_PLACES);
                }
            }
        }

        /// <summary>
        /// Report option
        /// </summary>
        private bool ShowReportOptional()
        {
            reportPopup = new ReportOptional();
            // Set DataContext to view ReportOptional
            reportPopup.DataContext = new FilterReportViewModel(
                reportPopup, this, ReportModel.FormatFile, (short)Common.FilterWindow.ReportOptional);
            reportPopup.ShowDialog();
            // Return when user click Cancel
            if (currentStoreCode == -2)
            {
                return false;
            }
            // Show Report 
            if (!isPrint)
            {
                viewReportWindow.Show();
            }
            slectedStoreName = GetStoreName(currentStoreCode);
            // Get configuration 
            GetConfiguration();
            return true;
        }

        /// <summary>
        /// Report option
        /// </summary>
        private bool ShowPurchaseOptional()
        {
            purchaseOptional = new PurchaseOptional();
            // Set DataContext to view ReportOptional
            purchaseOptional.DataContext = new FilterReportViewModel(
                purchaseOptional, this, ReportModel.FormatFile, (short)Common.FilterWindow.PurchaseOPtional);
            purchaseOptional.ShowDialog();
            // Return when user Cancel
            if (currentStoreCode == -2)
            {
                return false;
            }
            // Show Report 
            if (!isPrint)
            {
                viewReportWindow.Show();
            }
            slectedStoreName = GetStoreName(currentStoreCode);
            // Get configuration 
            GetConfiguration();
            return true;
        }

        /// <summary>
        /// Report option
        /// </summary>
        private bool ShowCustomerPaymentOptional()
        {
            customerPaymentOptional = new CustomerPaymentOptional();
            // Set DataContext to view 
            customerPaymentOptional.DataContext = new FilterReportViewModel(
                customerPaymentOptional, this, ReportModel.FormatFile, (short)Common.FilterWindow.CustomerPaymentOptional);
            customerPaymentOptional.ShowDialog();
            // Return when user Cancel
            if (currentStoreCode == -2)
            {
                return false;
            }
            // Show Report 
            if (!isPrint)
            {
                viewReportWindow.Show();
            }
            slectedStoreName = GetStoreName(currentStoreCode);
            // Get configuration 
            GetConfiguration();
            return true;
        }

        #endregion

        #region -Load report data-

        #region -Inventory report-
        /// <summary>
        /// Get Product List
        /// </summary>
        private void GetProductList()
        {
            report.Tables[Common.DT_PRODUCT_LIST].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            // param to filter
            param = string.Format("{0},{1},{2}", currentStoreCode, categoryId, productResource);
            // Get product report by storecode
            da = dbHelp.ExecuteQuery("sp_inv_get_product_list", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                string storeName = string.Empty;
                if (da.Rows[i][10] != DBNull.Value)
                {
                    storeName = GetStoreName(int.Parse(da.Rows[i][10].ToString()));
                }
                report.Tables[Common.DT_PRODUCT_LIST].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], da.Rows[i][3], da.Rows[i][4], da.Rows[i][5], da.Rows[i][6],
                    da.Rows[i][7], da.Rows[i][8], da.Rows[i][9], storeName
                    );
            }
            da.Clear();
            // Set data Source to  report
            productListReport = new rptProductList();
            productListReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            productListReport.SetDataSource(report.Tables[Common.DT_PRODUCT_LIST]);
            // Set currency to report
            productListReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress item
                productListReport.Section2.ReportObjects["Line3"].ObjectFormat.EnableSuppress = true;
                productListReport.Section2.ReportObjects["Line25"].ObjectFormat.EnableSuppress = true;
                productListReport.Section2.ReportObjects["Text12"].ObjectFormat.EnableSuppress = true;
                productListReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;

                // Resize Item product name
                productListReport.Section2.ReportObjects["Text2"].Width = 3640;
                productListReport.Section2.ReportObjects["ItemName1"].Width = 3640;
                // Set store name to report                        
                productListReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                productListReport.Section2.ReportObjects["Text14"].ObjectFormat.EnableSuppress = true;
            }
            // Suppress item if have no data
            if (count == 0)
            {
                productListReport.Section3.ReportObjects["Text11"].ObjectFormat.EnableSuppress = true;
                productListReport.Section4.ReportObjects["Text15"].ObjectFormat.EnableSuppress = true;
                productListReport.Section3.SectionFormat.EnableSuppress = true;
            }
            ReportSource = productListReport;
        }
        /// <summary>
        /// Get Cost Adjustment
        /// </summary>
        private void GetCostAdjustment()
        {
            report.Tables[Common.DT_QTY_COST_ADJUSTMENT].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4},{5},{6}", currentStoreCode, categoryId, productResource, adjustmentStatus, adjustmentReason, fromDate, toDate);
            // Get all Cost Adjustment by store
            da = dbHelp.ExecuteQuery("sp_inv_get_cost_adjustment", param);
            int count = da.Rows.Count;
            // add data to data set
            for (int i = 0; i < count; i++)
            {
                string date = Common.ToShortDateString(da.Rows[i][6]);
                report.Tables[Common.DT_QTY_COST_ADJUSTMENT].Rows.Add(
                    da.Rows[i][0], xmlHelper.GetName(int.Parse(da.Rows[i][1].ToString()), "AdjustmentReason"),
                    xmlHelper.GetName(int.Parse(da.Rows[i][2].ToString()), "AdjustmentStatus"), da.Rows[i][3], da.Rows[i][4],
                    da.Rows[i][5], date, da.Rows[i][7], da.Rows[i][8], da.Rows[i][9], GetStoreName(int.Parse(da.Rows[i][10].ToString()))
                    );
            }
            // Clear data in table
            da.Clear();
            // Set data source
            costAdjustmentReport = new rptCostAdjustment();
            costAdjustmentReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            costAdjustmentReport.SetDataSource(report.Tables[Common.DT_QTY_COST_ADJUSTMENT]);
            costAdjustmentReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Store name
                costAdjustmentReport.Section2.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                costAdjustmentReport.Section2.ReportObjects["Line2"].ObjectFormat.EnableSuppress = true;
                costAdjustmentReport.Section2.ReportObjects["Text12"].ObjectFormat.EnableSuppress = true;
                costAdjustmentReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize product name 
                costAdjustmentReport.Section2.ReportObjects["Text5"].Width = 3640;
                costAdjustmentReport.Section2.ReportObjects["ItemName1"].Width = 3640;
                costAdjustmentReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                costAdjustmentReport.Section2.ReportObjects["Text14"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                costAdjustmentReport.Section2.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
                costAdjustmentReport.Section3.ReportObjects["Text13"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = costAdjustmentReport;
        }
        /// <summary>
        /// Get Quantity Adjustment
        /// </summary>
        private void GetQuantityAdjustment()
        {
            report.Tables[Common.DT_QTY_COST_ADJUSTMENT].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4},{5},{6}", currentStoreCode, categoryId, productResource, adjustmentStatus, adjustmentReason, fromDate, toDate);
            // Get all Quantity Adjustment
            da = dbHelp.ExecuteQuery("sp_inv_get_quantity_adjustment", param);
            int count = da.Rows.Count;
            // add data to data set
            for (int i = 0; i < count; i++)
            {
                string date = Common.ToShortDateString(da.Rows[i][6]);
                report.Tables[Common.DT_QTY_COST_ADJUSTMENT].Rows.Add(
                    da.Rows[i][0], xmlHelper.GetName(int.Parse(da.Rows[i][1].ToString()), "AdjustmentReason"),
                    xmlHelper.GetName(int.Parse(da.Rows[i][2].ToString()), "AdjustmentStatus"), da.Rows[i][3], da.Rows[i][4],
                    da.Rows[i][5], date, da.Rows[i][7], da.Rows[i][8], da.Rows[i][9], GetStoreName(int.Parse(da.Rows[i][10].ToString()))
                    );
            }
            // Clear data in table
            da.Clear();
            // Set data source
            quantityAdjustmentReport = new rptQuantityAdjustment();
            quantityAdjustmentReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            quantityAdjustmentReport.SetDataSource(report.Tables[Common.DT_QTY_COST_ADJUSTMENT]);
            quantityAdjustmentReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Store name
                quantityAdjustmentReport.Section2.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                quantityAdjustmentReport.Section2.ReportObjects["Line2"].ObjectFormat.EnableSuppress = true;
                quantityAdjustmentReport.Section2.ReportObjects["Text12"].ObjectFormat.EnableSuppress = true;
                quantityAdjustmentReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize Item product name 
                quantityAdjustmentReport.Section2.ReportObjects["Text5"].Width = 3670;
                quantityAdjustmentReport.Section2.ReportObjects["ItemName1"].Width = 3670;
                quantityAdjustmentReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                quantityAdjustmentReport.Section2.ReportObjects["Text14"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                quantityAdjustmentReport.Section2.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
                quantityAdjustmentReport.Section3.ReportObjects["Text13"].ObjectFormat.EnableSuppress = true;
                quantityAdjustmentReport.Section2.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = quantityAdjustmentReport;
        }
        /// <summary>
        /// Get Product Summary Activivy
        /// </summary>
        private void GetProductSummaryActitivity()
        {
            report.Tables[Common.DT_PRODUCT_SUMMARY_ACTIVITY].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1}", currentStoreCode, categoryId);
            // Get Product Store by store code
            da = dbHelp.ExecuteQuery("sp_inv_get_product_summary_with_activity", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                report.Tables[Common.DT_PRODUCT_SUMMARY_ACTIVITY].Rows.Add(
                    da.Rows[i][0], GetStoreName(int.Parse(da.Rows[i][1].ToString())), da.Rows[i][2], da.Rows[i][3], da.Rows[i][4], da.Rows[i][5]
                    );
            }
            // Set data source
            productSummaryActivity = new rptProductSummaryActivity();
            productSummaryActivity.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            productSummaryActivity.SetDataSource(report.Tables[Common.DT_PRODUCT_SUMMARY_ACTIVITY]);
            productSummaryActivity.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            // Suppress Store name
            if (currentStoreCode != -1)
            {
                // Supppress Store name
                productSummaryActivity.Section2.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                productSummaryActivity.Section2.ReportObjects["Text2"].ObjectFormat.EnableSuppress = true;
                productSummaryActivity.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize item Category name
                productSummaryActivity.Section2.ReportObjects["Line7"].Left = 720;
                productSummaryActivity.Section2.ReportObjects["Text1"].Left = 750;
                productSummaryActivity.Section2.ReportObjects["Text1"].Width = 3920;
                productSummaryActivity.Section2.ReportObjects["Text1"].Top = 120;
                productSummaryActivity.Section2.ReportObjects["CategoryName1"].Left = 750;
                productSummaryActivity.Section2.ReportObjects["CategoryName1"].Width = 3920;
                productSummaryActivity.Section2.ReportObjects["CategoryName1"].Top = 120;
                productSummaryActivity.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                productSummaryActivity.Section2.ReportObjects["Text9"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                productSummaryActivity.Section4.ReportObjects["Text10"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = productSummaryActivity;
        }
        /// <summary>
        /// Get Category List
        /// </summary>
        private void GetCategoryList()
        {
            report.Tables[Common.DT_DEPARTMENT].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1}", departId, categoryId);
            //// Get category list by store                                                
            da = dbHelp.ExecuteQuery("sp_inv_get_category_list", param);
            int count = da.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                report.Tables[Common.DT_DEPARTMENT].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], da.Rows[i][3], da.Rows[i][4]
                    );
            }
            // Set data Source to report
            categoryListReport = new rptCategoryList();
            categoryListReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            categoryListReport.SetDataSource(report.Tables[Common.DT_DEPARTMENT]);
            if (count == 0)
            {
                // Suppress item
                categoryListReport.Section3.ReportObjects["gDepart"].ObjectFormat.EnableSuppress = true;
                categoryListReport.Section3.SectionFormat.EnableSuppress = true;
                categoryListReport.Section3.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
                categoryListReport.Section3.ReportObjects["Line12"].ObjectFormat.EnableSuppress = true;
                categoryListReport.Section3.ReportObjects["Line14"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = categoryListReport;
        }
        /// <summary>
        /// Get ReOrder Stock
        /// </summary>
        private void GetReOrderStock()
        {
            report.Tables[Common.DT_REORDER_STOCK].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1}", currentStoreCode, productResource);
            // Get all Product store
            da = dbHelp.ExecuteQuery("sp_inv_get_reorder_stock", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                report.Tables[Common.DT_REORDER_STOCK].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], da.Rows[i][3], da.Rows[i][4], da.Rows[i][5],
                    da.Rows[i][6], da.Rows[i][7], da.Rows[i][8], da.Rows[i][9]
                    );
            }
            // Clear data
            da.Clear();
            // Set data source
            reOrderStockReport = new rptReOrderStock();
            reOrderStockReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            reOrderStockReport.SetDataSource(report.Tables[Common.DT_REORDER_STOCK]);
            if (currentStoreCode != -1)
            {
                reOrderStockReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                reOrderStockReport.Section2.ReportObjects["Text12"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                reOrderStockReport.Section2.ReportObjects["Line12"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = reOrderStockReport;
        }
        /// <summary>
        /// Get Transfer History
        /// </summary>
        private void GetTransferHistory()
        {
            report.Tables[Common.DT_TRANSFER_HISTORY].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4}", currentStoreCode, transferTo, transferStockStatus, fromDate, toDate);
            // Get all Transfer history from store        
            da = dbHelp.ExecuteQuery("sp_inv_get_transfer_stock", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string dateCreated = Common.ToShortDateString(da.Rows[i][1]);
                // Get status from XML
                string status = xmlHelper.GetName(int.Parse(da.Rows[i][2].ToString()), "TransferStockStatus");
                // Get from store
                string fromStore = GetStoreName(int.Parse(da.Rows[i][3].ToString()));
                // Get to store
                string toStore = GetStoreName(int.Parse(da.Rows[i][4].ToString()));
                // Format date applied
                string dateApplied = Common.ToShortDateString(da.Rows[i][7]);
                // Format date reversed
                string dateReversed = Common.ToShortDateString(da.Rows[i][9]);
                report.Tables[Common.DT_TRANSFER_HISTORY].Rows.Add(
                    da.Rows[i][0], dateCreated, status, fromStore, toStore, da.Rows[i][5],
                    da.Rows[i][6], dateApplied, da.Rows[i][8], dateReversed, da.Rows[i][10]
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            transferHistoryReport = new rptTransferHistory();
            transferHistoryReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            transferHistoryReport.SetDataSource(report.Tables[Common.DT_TRANSFER_HISTORY]);
            if (currentStoreCode != -1)
            {
                transferHistoryReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                transferHistoryReport.Section2.ReportObjects["Text12"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                transferHistoryReport.Section2.ReportObjects["Text14"].ObjectFormat.EnableSuppress = true;
                transferHistoryReport.Section2.ReportObjects["Line28"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = transferHistoryReport;
        }
        /// <summary>
        /// Get Transfer History Details
        /// </summary>
        private void GetTransferHistoryDetails()
        {
            report.Tables[Common.DT_TRANSFER_DETAILS].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2}", currentStoreCode, categoryId, productResource);
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("sp_inv_get_transfer_stock_details", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                report.Tables[Common.DT_TRANSFER_DETAILS].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], GetStoreName(int.Parse(da.Rows[i][3].ToString())),
                    da.Rows[i][4], da.Rows[i][5], da.Rows[i][6], da.Rows[i][7], da.Rows[i][8], da.Rows[i][9]
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            transferHistoryDetailsReport = new rptTransferHistoryDetails();
            transferHistoryDetailsReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            transferHistoryDetailsReport.SetDataSource(report.Tables[Common.DT_TRANSFER_DETAILS]);
            transferHistoryDetailsReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress control
                transferHistoryDetailsReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
                transferHistoryDetailsReport.Section3.ReportObjects["Text5"].ObjectFormat.EnableSuppress = true;
                transferHistoryDetailsReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                transferHistoryDetailsReport.Section3.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                transferHistoryDetailsReport.Section3.ReportObjects["Line15"].ObjectFormat.EnableSuppress = true;

                // Resise product name
                transferHistoryDetailsReport.Section3.ReportObjects["Text4"].Width = 3900;
                transferHistoryDetailsReport.Section3.ReportObjects["ProductName1"].Width = 3900;
            }
            else
            {
                transferHistoryDetailsReport.Section2.ReportObjects["Text14"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                transferHistoryDetailsReport.Section3.SectionFormat.EnableSuppress = true;
                transferHistoryDetailsReport.Section1.ReportObjects["Text15"].ObjectFormat.EnableSuppress = true;
                transferHistoryDetailsReport.Section4.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
                transferHistoryDetailsReport.Section2.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = transferHistoryDetailsReport;
        }
        #endregion

        #region -Purchasing report-
        /// <summary>
        /// Get Purchase Order Summary
        /// </summary>
        private void GetPOSummary()
        {
            report.Tables[Common.DT_PO_SUMMARY].Clear();
            if (!ShowPurchaseOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4}", currentStoreCode, customerResource, purchaseOrderStatus, fromDate, toDate);
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("sp_pur_get_po_summary", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format purchase date 
                string purchaseDate = Common.ToShortDateString(da.Rows[i][4]);
                // Format ship date 
                string shipDate = Common.ToShortDateString(da.Rows[i][5]);
                // Format due date 
                string dueDate = Common.ToShortDateString(da.Rows[i][6]);
                // Get Purchase Status
                string purchaseStatus = xmlHelper.GetName(int.Parse(da.Rows[i][3].ToString()), "PurchaseStatus");
                report.Tables[Common.DT_PO_SUMMARY].Rows.Add(
                da.Rows[i][0], da.Rows[i][1], GetStoreName(int.Parse(da.Rows[i][2].ToString())), purchaseStatus,
                purchaseDate, shipDate, dueDate, da.Rows[i][7], da.Rows[i][8], da.Rows[i][9]
                );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            pOSummaryReport = new rptPOSummary();
            pOSummaryReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            pOSummaryReport.SetDataSource(report.Tables[Common.DT_PO_SUMMARY]);
            pOSummaryReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress control
                pOSummaryReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
                pOSummaryReport.Section3.ReportObjects["Text3"].ObjectFormat.EnableSuppress = true;
                pOSummaryReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                pOSummaryReport.Section3.ReportObjects["Line10"].ObjectFormat.EnableSuppress = true;

                // Resise Vendor name
                pOSummaryReport.Section3.ReportObjects["Text2"].Width = 4165;
                pOSummaryReport.Section3.ReportObjects["Vendor1"].Width = 4165;
            }
            else
            {
                pOSummaryReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                pOSummaryReport.Section3.SectionFormat.EnableSuppress = true;
                pOSummaryReport.Section4.ReportObjects["Text12"].ObjectFormat.EnableSuppress = true;
                pOSummaryReport.Section2.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = pOSummaryReport;
        }
        /// <summary>
        /// Get Purchase Order Details
        /// </summary>
        private void GetPODetails()
        {
            report.Tables[Common.DT_PO_DETAILS].Clear();
            if (!ShowPurchaseOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4}", currentStoreCode, productResource, purchaseOrderStatus, fromDate, toDate);
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("sp_pur_get_po_details", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                string orderDate = Common.ToShortDateString(da.Rows[i][3]);
                string pOStatus = xmlHelper.GetName(int.Parse(da.Rows[i][4].ToString()), "PurchaseStatus");
                report.Tables[Common.DT_PO_DETAILS].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], GetStoreName(int.Parse(da.Rows[i][2].ToString())),
                    orderDate, pOStatus, da.Rows[i][5], da.Rows[i][6]
                    );
            }
            // Set data source
            pODetailsReport = new rptPODetails();
            pODetailsReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            pODetailsReport.SetDataSource(report.Tables[Common.DT_PO_DETAILS]);
            pODetailsReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress control                
                pODetailsReport.Section3.ReportObjects["Text4"].ObjectFormat.EnableSuppress = true;
                pODetailsReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                pODetailsReport.Section3.ReportObjects["Line11"].ObjectFormat.EnableSuppress = true;
                pODetailsReport.Section3.ReportObjects["Line5"].ObjectFormat.EnableSuppress = true;
                // Resise product name
                pODetailsReport.Section3.ReportObjects["Text3"].Width = 4480;
                pODetailsReport.Section3.ReportObjects["ProductName1"].Width = 4480;
                pODetailsReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                pODetailsReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                pODetailsReport.Section4.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
                pODetailsReport.Section2.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = pODetailsReport;
        }
        /// <summary>
        /// Get Product Cost
        /// </summary>
        private void GetProductCost()
        {
            report.Tables[Common.DT_PRODUCT_COST].Clear();
            if (!ShowPurchaseOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4},{5}", currentStoreCode, customerResource, categoryId, productResource, fromDate, toDate);
            // Get all Sale Order
            da = dbHelp.ExecuteQuery("sp_pur_get_product_cost", param);
            int count = da.Rows.Count;
            // Add data to dataset
            for (int i = 0; i < count; i++)
            {
                string pOdate = Common.ToShortDateString(da.Rows[i][3]);
                report.Tables[Common.DT_PRODUCT_COST].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], pOdate, da.Rows[i][4],
                    GetStoreName(int.Parse(da.Rows[i][5].ToString())), da.Rows[i][6], da.Rows[i][7], da.Rows[i][8]
                    );
            }
            // Set data source
            productCostReport = new rptProductCost();
            productCostReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            productCostReport.SetDataSource(report.Tables[Common.DT_PRODUCT_COST]);
            productCostReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Store name
                productCostReport.Section2.ReportObjects["Line7"].ObjectFormat.EnableSuppress = true;
                productCostReport.Section2.ReportObjects["Line14"].ObjectFormat.EnableSuppress = true;
                productCostReport.Section2.ReportObjects["Text7"].ObjectFormat.EnableSuppress = true;
                productCostReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize Vendor name 
                productCostReport.Section2.ReportObjects["Text6"].Width = 4020;
                productCostReport.Section2.ReportObjects["Vendor1"].Width = 4020;
                productCostReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                productCostReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                productCostReport.Section2.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
                productCostReport.Section2.ReportObjects["Line3"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = productCostReport;
        }
        /// <summary>
        /// Get Vendor Product List
        /// </summary>
        private void GetVendorProductList()
        {
            report.Tables[Common.DT_VENDOR_PRODUCT_LIST].Clear();
            if (!ShowPurchaseOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3}", currentStoreCode, customerResource, categoryId, productResource);
            // Get all Sale Order
            da = dbHelp.ExecuteQuery("sp_pur_get_vendor_product_list", param);
            int count = da.Rows.Count;
            // Add data to dataset
            for (int i = 0; i < count; i++)
            {
                report.Tables[Common.DT_VENDOR_PRODUCT_LIST].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], GetStoreName(int.Parse(da.Rows[i][2].ToString())), da.Rows[i][3], da.Rows[i][4]
                    );
            }
            // Set data source
            vendorProductListReport = new rptVendorProductList();
            vendorProductListReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            vendorProductListReport.SetDataSource(report.Tables[Common.DT_VENDOR_PRODUCT_LIST]);
            vendorProductListReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Store name
                vendorProductListReport.Section2.ReportObjects["Line7"].ObjectFormat.EnableSuppress = true;
                vendorProductListReport.Section2.ReportObjects["Line10"].ObjectFormat.EnableSuppress = true;
                vendorProductListReport.Section2.ReportObjects["Text3"].ObjectFormat.EnableSuppress = true;
                vendorProductListReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize Product name 
                vendorProductListReport.Section2.ReportObjects["Text2"].Width = 4400;
                vendorProductListReport.Section2.ReportObjects["ProductName1"].Width = 4400;
                vendorProductListReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                vendorProductListReport.Section2.ReportObjects["Text14"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                vendorProductListReport.Section2.ReportObjects["Text12"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = vendorProductListReport;
        }
        /// <summary>
        /// Get Vendor List
        /// </summary>
        private void GetVendorList()
        {
            report.Tables[Common.DT_VENDOR_LIST].Clear();
            if (!ShowPurchaseOptional())
            {
                return;
            }
            param = string.Format("{0},{1}", countryValue, customerResource);
            // Get all Sale Order
            da = dbHelp.ExecuteQuery("sp_pur_get_vendor_list", param);
            int count = da.Rows.Count;
            // Add data to dataset
            for (int i = 0; i < count; i++)
            {
                string stateName = xmlHelper.GetName(int.Parse(da.Rows[i][4].ToString()), "State");
                string countryName = xmlHelper.GetName(int.Parse(da.Rows[i][6].ToString()), "Country");
                report.Tables[Common.DT_VENDOR_LIST].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], da.Rows[i][3], stateName, da.Rows[i][5], countryName
                    );
            }
            // Set data source
            vendorListReport = new rptVendorList();
            vendorListReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            vendorListReport.SetDataSource(report.Tables[Common.DT_VENDOR_LIST]);
            ReportSource = vendorListReport;
        }

        /// <summary>
        /// Get Purchase Order Locked
        /// </summary>
        private void GetPOLocked()
        {
            report.Tables[Common.DT_SOPO_LOCKED].Clear();
            if (!isPrint)
            {
                viewReportWindow.Show();
            }
            GetConfiguration();
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("v_rpt_pur_get_po_locked");
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string date = Common.ToShortDateString(da.Rows[i][1]);
                string saleOrderStatus = xmlHelper.GetName(int.Parse(da.Rows[i][2].ToString()), "PurchaseStatus");
                report.Tables[Common.DT_SOPO_LOCKED].Rows.Add(
                        da.Rows[i][0], date, saleOrderStatus, da.Rows[i][3], da.Rows[i][4],
                        GetStoreName(int.Parse(da.Rows[i][5].ToString())), da.Rows[i][6]
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            pOLockedReport = new rptPOLocked();
            pOLockedReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            pOLockedReport.SetDataSource(report.Tables[Common.DT_SOPO_LOCKED]);
            pOLockedReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (count == 0)
            {
                pOLockedReport.Section2.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
                pOLockedReport.Section4.ReportObjects["lblGrandTotal"].ObjectFormat.EnableSuppress = true;
                pOLockedReport.Section3.SectionFormat.EnableSuppress = true;
            }
            ReportSource = pOLockedReport;
        }
        #endregion

        #region -Sale report-
        /// <summary>
        /// Get Sale By Product Summary
        /// </summary>
        private void GetSaleByProductSummary()
        {
            report.Tables[Common.DT_SALE_BY_PRODUCT_SUMMARY].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2}", currentStoreCode, categoryId, productResource);
            // Get all Sale Order
            da = dbHelp.ExecuteQuery("sp_sale_get_sale_by_product_summary", param);
            int count = da.Rows.Count;
            // Add data to dataset
            for (int i = 0; i < count; i++)
            {
                report.Tables[Common.DT_SALE_BY_PRODUCT_SUMMARY].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], GetStoreName(int.Parse(da.Rows[i][3].ToString())), da.Rows[i][4], da.Rows[i][5], da.Rows[i][6], da.Rows[i][7],
                    da.Rows[i][8], da.Rows[i][9], da.Rows[i][10], da.Rows[i][11], da.Rows[i][12]
                    );
            }
            // Clear data in table
            da.Clear();
            // Set data source
            salebyProductSummaryReport = new rptSaleByProductSummary();
            salebyProductSummaryReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            salebyProductSummaryReport.SetDataSource(report.Tables[Common.DT_SALE_BY_PRODUCT_SUMMARY]);
            salebyProductSummaryReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress store name and line
                salebyProductSummaryReport.Section2.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                salebyProductSummaryReport.Section2.ReportObjects["Line18"].ObjectFormat.EnableSuppress = true;
                salebyProductSummaryReport.Section2.ReportObjects["Text5"].ObjectFormat.EnableSuppress = true;
                salebyProductSummaryReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize Item product name 
                salebyProductSummaryReport.Section2.ReportObjects["Text4"].Width = 3420;
                salebyProductSummaryReport.Section2.ReportObjects["ProductName1"].Width = 3420;
                salebyProductSummaryReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                salebyProductSummaryReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            // Suppress object (if no record found) 
            if (count == 0)
            {
                salebyProductSummaryReport.Section3.ReportObjects["Text17"].ObjectFormat.EnableSuppress = true;
                salebyProductSummaryReport.Section3.ReportObjects["Field1"].ObjectFormat.EnableSuppress = true;
                salebyProductSummaryReport.Section4.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = salebyProductSummaryReport;
        }
        /// <summary>
        /// Get Sale By Product Details
        /// </summary>
        private void GetSaleByProductDetails()
        {
            report.Tables[Common.DT_SALE_BY_PRODUCT_DETAILS].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4}", currentStoreCode, categoryId, productResource, fromDate, toDate);
            // get data
            da = dbHelp.ExecuteQuery("sp_sale_get_sale_by_product_details", param);
            int count = da.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                string orderDate = Common.ToShortDateString(da.Rows[i][6]);
                report.Tables[Common.DT_SALE_BY_PRODUCT_DETAILS].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], GetStoreName(int.Parse(da.Rows[i][3].ToString())),
                    da.Rows[i][4], da.Rows[i][5], orderDate, da.Rows[i][7], da.Rows[i][8], da.Rows[i][9], da.Rows[i][10]
                    );
            }
            da.Clear();
            salebyProductDetailsReport = new rptSaleByProductDetails();
            salebyProductDetailsReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            salebyProductDetailsReport.SetDataSource(report.Tables[Common.DT_SALE_BY_PRODUCT_DETAILS]);
            salebyProductDetailsReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress store name and line
                salebyProductDetailsReport.Section1.ReportObjects["Line15"].ObjectFormat.EnableSuppress = true;
                salebyProductDetailsReport.Section3.ReportObjects["Line7"].ObjectFormat.EnableSuppress = true;
                salebyProductDetailsReport.Section2.ReportObjects["Text5"].ObjectFormat.EnableSuppress = true;
                salebyProductDetailsReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize product name
                salebyProductDetailsReport.Section2.ReportObjects["Text4"].Width = 4260;
                salebyProductDetailsReport.Section3.ReportObjects["ItemName1"].Width = 4260;
                salebyProductDetailsReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                salebyProductDetailsReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                salebyProductDetailsReport.Section3.ReportObjects["Text14"].ObjectFormat.EnableSuppress = true;
                salebyProductDetailsReport.Section4.ReportObjects["Text15"].ObjectFormat.EnableSuppress = true;
                salebyProductDetailsReport.Section3.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
                salebyProductDetailsReport.Section3.SectionFormat.EnableSuppress = true;
            }
            ReportSource = salebyProductDetailsReport;
        }
        /// <summary>
        /// Get Sale Order Summary
        /// </summary>
        private void GetSaleOrderSummary()
        {
            report.Tables[Common.DT_SALE_ORDER].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4}", currentStoreCode, customerResource, saleOrderStatus, fromDate, toDate);
            // GEt all Sale Order
            da = dbHelp.ExecuteQuery("sp_sale_get_sale_order_summary", param);
            int count = da.Rows.Count;
            // Add data to dataset
            for (int i = 0; i < count; i++)
            {
                string orderDate = Common.ToShortDateString(da.Rows[i][3]);
                string status = xmlHelper.GetName(int.Parse(da.Rows[i][4].ToString()), "SalesOrdersStatus");
                report.Tables[Common.DT_SALE_ORDER].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], GetStoreName(int.Parse(da.Rows[i][2].ToString())), orderDate, status,
                    da.Rows[i][5], da.Rows[i][6], da.Rows[i][7], da.Rows[i][8], da.Rows[i][9], da.Rows[i][10]
                    );
            }
            // Clear data in table
            da.Clear();
            // Set data source
            saleOrderSummaryReport = new rptSaleOrderSummary();
            saleOrderSummaryReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            saleOrderSummaryReport.SetDataSource(report.Tables[Common.DT_SALE_ORDER]);
            saleOrderSummaryReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress Store name
                saleOrderSummaryReport.Section2.ReportObjects["Line3"].ObjectFormat.EnableSuppress = true;
                saleOrderSummaryReport.Section2.ReportObjects["Text20"].ObjectFormat.EnableSuppress = true;
                saleOrderSummaryReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize Item Customer name 
                saleOrderSummaryReport.Section2.ReportObjects["Text12"].Width = 3380;
                saleOrderSummaryReport.Section2.ReportObjects["Customer1"].Width = 3380;
                saleOrderSummaryReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                saleOrderSummaryReport.Section2.ReportObjects["Text11"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                saleOrderSummaryReport.Section2.ReportObjects["Text9"].ObjectFormat.EnableSuppress = true;
                saleOrderSummaryReport.Section2.ReportObjects["Line9"].ObjectFormat.EnableSuppress = true;
                saleOrderSummaryReport.Section3.SectionFormat.EnableSuppress = true;
            }
            ReportSource = saleOrderSummaryReport;
        }
        /// <summary>
        /// Get Sale Profit Summary
        /// </summary>
        private void GetSaleProfitSummary()
        {
            report.Tables[Common.DT_SALE_PROFIT_SUMMARY].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4}", currentStoreCode, customerResource, saleOrderStatus, fromDate, toDate);
            // GEt all Sale Order
            da = dbHelp.ExecuteQuery("sp_sale_get_sale_profit_summary", param);
            int count = da.Rows.Count;
            // Add data to dataset
            for (int i = 0; i < count; i++)
            {
                string invoiceDate = Common.ToShortDateString(da.Rows[i][4]);
                string status = xmlHelper.GetName(int.Parse(da.Rows[i][1].ToString()), "SalesOrdersStatus");
                report.Tables[Common.DT_SALE_PROFIT_SUMMARY].Rows.Add(
                    da.Rows[i][0], status, da.Rows[i][2], GetStoreName(int.Parse(da.Rows[i][3].ToString())),
                    invoiceDate, da.Rows[i][5], da.Rows[i][7]
                    );
            }
            // Clear data in table
            da.Clear();
            // Set data source
            saleProfitSummaryReport = new rptSaleProfitSummary();
            saleProfitSummaryReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            saleProfitSummaryReport.SetDataSource(report.Tables[Common.DT_SALE_PROFIT_SUMMARY]);
            saleProfitSummaryReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress Store name
                saleProfitSummaryReport.Section2.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                saleProfitSummaryReport.Section2.ReportObjects["Text4"].ObjectFormat.EnableSuppress = true;
                saleProfitSummaryReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize Item Customer name 
                saleProfitSummaryReport.Section2.ReportObjects["Text3"].Width = 3400;
                saleProfitSummaryReport.Section2.ReportObjects["Customer1"].Width = 3400;
                saleProfitSummaryReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                saleProfitSummaryReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                saleProfitSummaryReport.Section3.SectionFormat.EnableSuppress = true;
                saleProfitSummaryReport.Section4.ReportObjects["lblGrandTotal"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = saleProfitSummaryReport;
        }
        /// <summary>
        /// Get Sale Order Operation
        /// </summary>
        private void GetSaleOrderOperational()
        {
            report.Tables[Common.DT_SALE_ORDER_OPERATION].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4}", currentStoreCode, customerResource, saleOrderStatus, fromDate, toDate);
            // GEt all Sale Order
            da = dbHelp.ExecuteQuery("sp_sale_get_sale_order_operational", param);
            int count = da.Rows.Count;
            // Add data to dataset
            for (int i = 0; i < count; i++)
            {
                string orderDate = Common.ToShortDateString(da.Rows[i][4]);
                string status = xmlHelper.GetName(int.Parse(da.Rows[i][1].ToString()), "SalesOrdersStatus");
                report.Tables[Common.DT_SALE_ORDER_OPERATION].Rows.Add(
                    da.Rows[i][0], status, da.Rows[i][2], GetStoreName(int.Parse(da.Rows[i][3].ToString())),
                    orderDate, da.Rows[i][5]
                    );
            }
            // Clear data in table
            da.Clear();
            // Set data source
            saleOrderOperationReport = new rptSaleOrderOperational();
            saleOrderOperationReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            saleOrderOperationReport.SetDataSource(report.Tables[Common.DT_SALE_ORDER_OPERATION]);
            saleOrderOperationReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress Store name
                saleOrderOperationReport.Section2.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                saleOrderOperationReport.Section2.ReportObjects["Line13"].ObjectFormat.EnableSuppress = true;
                saleOrderOperationReport.Section2.ReportObjects["Text5"].ObjectFormat.EnableSuppress = true;
                saleOrderOperationReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize Item Customer name 
                saleOrderOperationReport.Section2.ReportObjects["Text5"].Width = 5830;
                saleOrderOperationReport.Section2.ReportObjects["Customer1"].Width = 5830;
                saleOrderOperationReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                saleOrderOperationReport.Section2.ReportObjects["Text15"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                saleOrderOperationReport.Section3.SectionFormat.EnableSuppress = true;
                saleOrderOperationReport.Section1.ReportObjects["Text11"].ObjectFormat.EnableSuppress = true;
                saleOrderOperationReport.Section4.ReportObjects["Text14"].ObjectFormat.EnableSuppress = true;
                saleOrderOperationReport.Section4.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = saleOrderOperationReport;
        }
        /// <summary>
        /// Get Customer Payment Summary
        /// </summary>
        private void GetCustomerPaymentSummary()
        {
            report.Tables[Common.DT_CUSTOMER_PAYMENT_SUMMARY].Clear();
            if (!ShowCustomerPaymentOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4},{5}", currentStoreCode, customerResource, fromDate, toDate, shipFrom, shipTo);
            // GEt all Sale Order
            da = dbHelp.ExecuteQuery("sp_sale_get_customer_payment_summary", param);
            int count = da.Rows.Count;
            // Add data to dataset
            for (int i = 0; i < count; i++)
            {
                string lastOrder = Common.ToShortDateString(da.Rows[i][5]);
                string lastPayment = Common.ToShortDateString(da.Rows[i][6]);
                report.Tables[Common.DT_CUSTOMER_PAYMENT_SUMMARY].Rows.Add(
                    da.Rows[i][0], GetStoreName(int.Parse(da.Rows[i][1].ToString())), da.Rows[i][2],
                    da.Rows[i][3], da.Rows[i][4], lastOrder, lastPayment
                    );
            }
            // Clear data in table
            da.Clear();
            // Set data source
            customerPaymentSummaryReport = new rptCustomerPaymentSummary();
            customerPaymentSummaryReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            customerPaymentSummaryReport.SetDataSource(report.Tables[Common.DT_CUSTOMER_PAYMENT_SUMMARY]);
            customerPaymentSummaryReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress Store name
                customerPaymentSummaryReport.Section2.ReportObjects["Line4"].ObjectFormat.EnableSuppress = true;
                customerPaymentSummaryReport.Section2.ReportObjects["Text2"].ObjectFormat.EnableSuppress = true;
                customerPaymentSummaryReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                // Resize Item Customer name 
                customerPaymentSummaryReport.Section2.ReportObjects["Text1"].Width = 3690;
                customerPaymentSummaryReport.Section2.ReportObjects["Customer1"].Width = 3690;
                customerPaymentSummaryReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
            }
            else
            {
                customerPaymentSummaryReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                customerPaymentSummaryReport.Section4.ReportObjects["Text9"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = customerPaymentSummaryReport;
        }
        /// <summary>
        /// Get Customer Payment Details
        /// </summary>
        private void GetCustomerPaymentDetails()
        {
            report.Tables[Common.DT_CUSTOMER_PAYMENT_DETAILS].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4},{5},{6}", currentStoreCode, customerResource, saleOrderStatus, fromDate, toDate, shipFrom, shipTo);
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("sp_sale_get_customer_payment_details", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string invoiceDate = Common.ToShortDateString(da.Rows[i][4]);
                // Format date paid
                string invoicePaid = Common.ToShortDateString(da.Rows[i][6]);
                string status = xmlHelper.GetName(int.Parse(da.Rows[i][2].ToString()), "SalesOrdersStatus");
                report.Tables[Common.DT_CUSTOMER_PAYMENT_DETAILS].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], status, GetStoreName(int.Parse(da.Rows[i][3].ToString())),
                     invoiceDate, da.Rows[i][5], invoicePaid, da.Rows[i][7], da.Rows[i][8], da.Rows[i][9]
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            customerPaymentDetailsReport = new rptCustomerPaymentDetails();
            customerPaymentDetailsReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            customerPaymentDetailsReport.SetDataSource(report.Tables[Common.DT_CUSTOMER_PAYMENT_DETAILS]);
            customerPaymentDetailsReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress control
                customerPaymentDetailsReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
                customerPaymentDetailsReport.Section3.ReportObjects["Text5"].ObjectFormat.EnableSuppress = true;
                customerPaymentDetailsReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                customerPaymentDetailsReport.Section3.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                customerPaymentDetailsReport.Section3.ReportObjects["Line16"].ObjectFormat.EnableSuppress = true;

                // Resise item Status name
                customerPaymentDetailsReport.Section3.ReportObjects["Text4"].Width = 4130;
                customerPaymentDetailsReport.Section3.ReportObjects["Status1"].Width = 4130;
            }
            else
            {
                customerPaymentDetailsReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                customerPaymentDetailsReport.Section3.SectionFormat.EnableSuppress = true;
                customerPaymentDetailsReport.Section4.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
                customerPaymentDetailsReport.Section4.ReportObjects["Line3"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = customerPaymentDetailsReport;
        }
        /// <summary>
        /// Get Product Customer
        /// </summary>
        private void GetProductCustomer()
        {
            report.Tables[Common.DT_PRODUCT_CUSTOMER].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4},{5},{6}", currentStoreCode, customerResource, categoryId, productResource, saleOrderStatus, fromDate, toDate);
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("sp_sale_get_product_customer", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string orderDate = Common.ToShortDateString(da.Rows[i][5]);
                string status = xmlHelper.GetName(int.Parse(da.Rows[i][6].ToString()), "SalesOrdersStatus");
                report.Tables[Common.DT_PRODUCT_CUSTOMER].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][3], da.Rows[i][4], GetStoreName(int.Parse(da.Rows[i][2].ToString())),
                     orderDate, status, da.Rows[i][7], da.Rows[i][8]
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            productCustomerReport = new rptProductCustomer();
            productCustomerReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            productCustomerReport.SetDataSource(report.Tables[Common.DT_PRODUCT_CUSTOMER]);
            if (currentStoreCode != -1)
            {
                // Suppress control
                productCustomerReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
                productCustomerReport.Section3.ReportObjects["Text6"].ObjectFormat.EnableSuppress = true;
                productCustomerReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                productCustomerReport.Section3.ReportObjects["Line5"].ObjectFormat.EnableSuppress = true;
                productCustomerReport.Section3.ReportObjects["Line13"].ObjectFormat.EnableSuppress = true;

                // Resise Customer name
                productCustomerReport.Section3.ReportObjects["Text5"].Width = 3720;
                productCustomerReport.Section3.ReportObjects["Customer1"].Width = 3720;
            }
            else
            {
                productCustomerReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                productCustomerReport.Section3.SectionFormat.EnableSuppress = true;
                productCustomerReport.Section4.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = productCustomerReport;
        }
        /// <summary>
        /// Get Customer Oder History
        /// </summary>
        private void GetCustomerOrderHistory()
        {
            report.Tables[Common.DT_CUSTOMER_ORDER_HISTORY].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4},{5},{6}", currentStoreCode, customerResource, categoryId, productResource, saleOrderStatus, fromDate, toDate);
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("sp_sale_customer_order_history", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string orderDate = Common.ToShortDateString(da.Rows[i][6]);
                string status = xmlHelper.GetName(int.Parse(da.Rows[i][5].ToString()), "SalesOrdersStatus");
                report.Tables[Common.DT_CUSTOMER_ORDER_HISTORY].Rows.Add(
                        da.Rows[i][1], da.Rows[i][0], da.Rows[i][2], da.Rows[i][3],
                        GetStoreName(int.Parse(da.Rows[i][4].ToString())),
                        orderDate, status, da.Rows[i][7], da.Rows[i][8]
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            customerOrderHistoryReport = new rptCustomerOrderHistory();
            customerOrderHistoryReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            customerOrderHistoryReport.SetDataSource(report.Tables[Common.DT_CUSTOMER_ORDER_HISTORY]);
            customerOrderHistoryReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress control
                customerOrderHistoryReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
                customerOrderHistoryReport.Section3.ReportObjects["Text6"].ObjectFormat.EnableSuppress = true;
                customerOrderHistoryReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                customerOrderHistoryReport.Section3.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                customerOrderHistoryReport.Section3.ReportObjects["Line13"].ObjectFormat.EnableSuppress = true;

                // Resise Category name
                customerOrderHistoryReport.Section3.ReportObjects["Text5"].Width = 3900;
                customerOrderHistoryReport.Section3.ReportObjects["Category1"].Width = 3900;
            }
            else
            {
                customerOrderHistoryReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                customerOrderHistoryReport.Section4.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
                customerOrderHistoryReport.Section3.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = customerOrderHistoryReport;
        }
        /// <summary>
        /// Get Sale RePresentative
        /// </summary>
        private void GetSaleRepresentative()
        {
            report.Tables[Common.DT_SALE_REPRESENTATIVE].Clear();
            if (!ShowReportOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2},{3},{4}", currentStoreCode, customerResource, saleOrderStatus, fromDate, toDate);
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("sp_sale_representative", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string invoiceDate = Common.ToShortDateString(da.Rows[i][6]);
                string status = xmlHelper.GetName(int.Parse(da.Rows[i][3].ToString()), "SalesOrdersStatus");
                report.Tables[Common.DT_SALE_REPRESENTATIVE].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], GetStoreName(int.Parse(da.Rows[i][2].ToString())), status,
                     da.Rows[i][4], da.Rows[i][5], invoiceDate
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            saleRepresentativeReport = new rptSaleRepresentative();
            saleRepresentativeReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            saleRepresentativeReport.SetDataSource(report.Tables[Common.DT_SALE_REPRESENTATIVE]);
            saleRepresentativeReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress control
                saleRepresentativeReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
                saleRepresentativeReport.Section3.ReportObjects["Text4"].ObjectFormat.EnableSuppress = true;
                saleRepresentativeReport.Section3.ReportObjects["StoreName1"].ObjectFormat.EnableSuppress = true;
                saleRepresentativeReport.Section3.ReportObjects["Line6"].ObjectFormat.EnableSuppress = true;
                saleRepresentativeReport.Section3.ReportObjects["Line12"].ObjectFormat.EnableSuppress = true;

                // Resise Status name
                saleRepresentativeReport.Section3.ReportObjects["Text5"].Width = 4200;
                saleRepresentativeReport.Section3.ReportObjects["Status1"].Width = 4200;
            }
            else
            {
                saleRepresentativeReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                saleRepresentativeReport.Section3.SectionFormat.EnableSuppress = true;
                saleRepresentativeReport.Section4.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = saleRepresentativeReport;
        }
        /// <summary>
        /// Get Sale Commission
        /// </summary>
        private void GetSaleCommission()
        {
            report.Tables[Common.DT_SALE_COMMISSION].Clear();
            if (!isPrint)
            {
                viewReportWindow.Show();
            }
            // Get configuration 
            GetConfiguration();
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("v_rpt_sale_commission");
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string invoiceDate = Common.ToShortDateString(da.Rows[i][4]);
                report.Tables[Common.DT_SALE_COMMISSION].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], GetStoreName(int.Parse(da.Rows[i][2].ToString())),
                    da.Rows[i][3], invoiceDate, da.Rows[i][5], da.Rows[i][6], da.Rows[i][7]
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            saleCommissionReport = new rptSaleCommission();
            saleCommissionReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            saleCommissionReport.SetDataSource(report.Tables[Common.DT_SALE_COMMISSION]);
            saleCommissionReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (count == 0)
            {
                saleCommissionReport.Section3.SectionFormat.EnableSuppress = true;
                saleCommissionReport.Section4.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = saleCommissionReport;
        }
        /// <summary>
        /// Get Sale Commission details
        /// </summary>
        private void GetSaleCommissionDetails()
        {
            report.Tables[Common.DT_SALE_COMMISSION_DETAILS].Clear();
            if (!isPrint)
            {
                viewReportWindow.Show();
            }
            // Get configuration 
            GetConfiguration();
            da = dbHelp.ExecuteQuery("v_rpt_sale_commission_details");
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string invoiceDate = Common.ToShortDateString(da.Rows[i][4]);
                report.Tables[Common.DT_SALE_COMMISSION_DETAILS].Rows.Add(
                    da.Rows[i][0], da.Rows[i][1], da.Rows[i][2], GetStoreName(int.Parse(da.Rows[i][3].ToString())),
                    invoiceDate, da.Rows[i][5], da.Rows[i][6], da.Rows[i][7]
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            saleCommissionDetailsReport = new rptSaleCommissionDetails();
            saleCommissionDetailsReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            saleCommissionDetailsReport.SetDataSource(report.Tables[Common.DT_SALE_COMMISSION_DETAILS]);
            saleCommissionDetailsReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (count == 0)
            {
                saleCommissionDetailsReport.Section2.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
                saleCommissionDetailsReport.Section4.ReportObjects["Text1"].ObjectFormat.EnableSuppress = true;
                //saleCommissionDetailsReport.Section3.SectionFormat.EnableSuppress = true;
            }
            ReportSource = saleCommissionDetailsReport;
        }
        /// <summary>
        /// Get Gift Certificate List
        /// </summary>
        private void GetGiftCertificateList()
        {
            report.Tables[Common.DT_GIFT_CERTIFICATE].Clear();
            if (!isPrint)
            {
                viewReportWindow.Show();
            }
            // Get configuration 
            GetConfiguration();
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("v_rpt_sale_card_management");
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string purchasedDate = Common.ToShortDateString(da.Rows[i][2]);
                // Format date create
                string lastUsedDate = Common.ToShortDateString(da.Rows[i][4]);
                // Format date create
                string createdDate = Common.ToShortDateString(da.Rows[i][10]);
                string paymentMethods = string.Empty;
                if (da.Rows[i][1] != DBNull.Value)
                {
                    paymentMethods = xmlHelper.GetName(int.Parse(da.Rows[i][1].ToString()), "PaymentMethods");
                }
                string status = string.Empty;
                if (da.Rows[i][8] != DBNull.Value)
                {
                    status = xmlHelper.GetName(int.Parse(da.Rows[i][8].ToString()), "StatusBasic");
                }
                ConvertImageToByteArray();
                byte[] isSold = null;
                if (da.Rows[i][9] != DBNull.Value && bool.Parse(da.Rows[i][9].ToString()))
                {
                    isSold = trueImg;
                }
                report.Tables[Common.DT_GIFT_CERTIFICATE].Rows.Add(
                        da.Rows[i][0], paymentMethods, purchasedDate, da.Rows[i][3],
                        lastUsedDate, da.Rows[i][5], da.Rows[i][6], da.Rows[i][7],
                        status, isSold, createdDate
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            giftCertificateReport = new rptGiftCertificateList();
            giftCertificateReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            giftCertificateReport.SetDataSource(report.Tables[Common.DT_GIFT_CERTIFICATE]);
            giftCertificateReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (count == 0)
            {
                giftCertificateReport.Section4.ReportObjects["Text15"].ObjectFormat.EnableSuppress = true;
                giftCertificateReport.Section3.SectionFormat.EnableSuppress = true;
            }
            ReportSource = giftCertificateReport;
        }
        /// <summary>
        /// Get Sale Commission details
        /// </summary>
        private void GetVoidedInvoice()
        {
            report.Tables[Common.DT_VOIDED_INVOICE].Clear();
            if (!ShowCustomerPaymentOptional())
            {
                return;
            }
            param = string.Format("{0},{1},{2}", currentStoreCode, fromDate, toDate);
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("sp_sale_get_voided_invoice", param);
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string date = Common.ToShortDateString(da.Rows[i][2]);
                string time = string.Empty;
                if (da.Rows[i][2] != DBNull.Value)
                {
                    time = Common.ToShortTimeString((DateTime)da.Rows[i][2]);
                }
                report.Tables[Common.DT_VOIDED_INVOICE].Rows.Add(
                        da.Rows[i][0], da.Rows[i][1], date, time, da.Rows[i][3], da.Rows[i][4],
                        GetStoreName(int.Parse(da.Rows[i][5].ToString()))
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            voidedInvoiceReport = new rptVoidedInvoice();
            voidedInvoiceReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            voidedInvoiceReport.SetDataSource(report.Tables[Common.DT_VOIDED_INVOICE]);
            voidedInvoiceReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (currentStoreCode != -1)
            {
                // Suppress control
                voidedInvoiceReport.DataDefinition.FormulaFields["StoreName"].Text = "'" + slectedStoreName + "'";
                voidedInvoiceReport.Section3.ReportObjects["Text7"].ObjectFormat.EnableSuppress = true;
                voidedInvoiceReport.Section3.ReportObjects["Store1"].ObjectFormat.EnableSuppress = true;
                voidedInvoiceReport.Section3.ReportObjects["Line10"].ObjectFormat.EnableSuppress = true;

                // Resise Customer name
                voidedInvoiceReport.Section3.ReportObjects["Text5"].Width = 4300;
                voidedInvoiceReport.Section3.ReportObjects["Reason1"].Width = 4300;
            }
            else
            {
                voidedInvoiceReport.Section2.ReportObjects["Text16"].ObjectFormat.EnableSuppress = true;
            }
            if (count == 0)
            {
                voidedInvoiceReport.Section4.ReportObjects["lblGrandTotal"].ObjectFormat.EnableSuppress = true;
            }
            ReportSource = voidedInvoiceReport;
        }

        /// <summary>
        /// Get Sale Order Locked
        /// </summary>
        private void GetSOLocked()
        {
            report.Tables[Common.DT_SOPO_LOCKED].Clear();
            if (!isPrint)
            {
                viewReportWindow.Show();
            }
            GetConfiguration();
            // Get all Transfer history details from store        
            da = dbHelp.ExecuteQuery("v_rpt_sale_get_so_locked");
            int count = da.Rows.Count;
            // Add data to data set
            for (int i = 0; i < count; i++)
            {
                // Format date create
                string date = Common.ToShortDateString(da.Rows[i][1]);
                string saleOrderStatus = xmlHelper.GetName(int.Parse(da.Rows[i][2].ToString()), "SalesOrdersStatus");
                report.Tables[Common.DT_SOPO_LOCKED].Rows.Add(
                        da.Rows[i][0], date, saleOrderStatus, da.Rows[i][3], da.Rows[i][4],
                        GetStoreName(int.Parse(da.Rows[i][5].ToString())), da.Rows[i][6]
                    );
            }
            // Clear data in table 
            da.Clear();
            // Set data source
            sOLockedReport = new rptSOLocked();
            sOLockedReport.Subreports[0].SetDataSource(report.Tables[Common.DT_HEADER]);
            sOLockedReport.SetDataSource(report.Tables[Common.DT_SOPO_LOCKED]);
            sOLockedReport.DataDefinition.FormulaFields[Common.RPT_CURRENCY_SYMBOL].Text = "'" + Common.CURRENT_SYMBOL + "'";
            if (count == 0)
            {
                sOLockedReport.Section2.ReportObjects["Line1"].ObjectFormat.EnableSuppress = true;
                sOLockedReport.Section4.ReportObjects["lblGrandTotal"].ObjectFormat.EnableSuppress = true;
                sOLockedReport.Section3.SectionFormat.EnableSuppress = true;
            }
            ReportSource = sOLockedReport;
        }
        #endregion
        #endregion

        #endregion

        #region -Private Method-

        #region -Get printed History List-
        /// <summary>
        /// Get printed History List
        /// </summary>
        private void GetPrindHistoryList()
        {
            try
            {
                string folderName = Directory.GetCurrentDirectory() + "\\PDF File\\" + ReportModel.Code;
                PrintedCollection.Clear();
                if (Directory.Exists(folderName))
                {
                    List<PrintedModel> lstPrinted = new List<PrintedModel>();
                    DirectoryInfo d = new DirectoryInfo(folderName);//Assuming Test is your Folder
                    FileInfo[] Files = d.GetFiles("*.pdf"); //Getting Text files
                    foreach (FileInfo file in Files)
                    {
                        PrintedModel printed = new PrintedModel();
                        printed.CreatedDate = file.CreationTime;
                        printed.FilePath = file.FullName;
                        printed.FileName = file.Name;
                        lstPrinted.Add(printed);
                    }
                    PrintedCollection = new ObservableCollection<PrintedModel>(lstPrinted.OrderByDescending(o => o.CreatedDate));
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion

        #region -Delete Old History Printed List-
        /// <summary>
        /// Delete Old History Printed List
        /// </summary>
        /// <param name="reportCode"></param>        
        private void DeleteOldHistoryPrintedList(string reportCode)
        {
            string folderName = Directory.GetCurrentDirectory() + "\\PDF File\\" + reportCode;
            if (PrintedCollection != null && PrintedCollection.Count > 0)
            {
                PrintedCollection.Clear();
            }
            if (Directory.Exists(folderName))
            {
                DirectoryInfo d = new DirectoryInfo(folderName);
                FileInfo[] Files = d.GetFiles("*.pdf");
                foreach (FileInfo file in Files)
                {
                    var dd = file.CreationTime.AddDays(Common.KEEP_LOG);
                    if (dd < DateTime.Now)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }
        #endregion

        #region -Print to printer-
        /// <summary>
        /// Print report 
        /// </summary>
        private void PrintToPrinter(CrystalDecisions.CrystalReports.Engine.ReportDocument obj, bool isLandscape)
        {
            try
            {
                // Get all printer
                System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Printer");
                // Check printer is avaiable
                foreach (System.Management.ManagementObject printer in searcher.Get())
                {
                    if (printer["Name"].ToString().Equals(CurrentPrinter, StringComparison.OrdinalIgnoreCase)
                        && bool.Parse(printer["WorkOffline"].ToString()) == true)
                    {
                        MessageBox.Show("Printer name \"" + CurrentPrinter + "\" can not be used now, please choose another printer.", "Warning",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                }
                // Set orientation
                obj.PrintOptions.PaperOrientation = isLandscape ? PaperOrientation.Landscape : PaperOrientation.Portrait;
                // Set printer name            
                obj.PrintOptions.PrinterName = CurrentPrinter;
                //Set page size
                obj.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)ReportModel.PaperSize;
                // Print report
                obj.PrintToPrinter(ReportModel.PrintCopy, true, 0, 0);
            }
            catch (Exception)
            {
                return;
            }
        }

        #endregion

        #region -Export report file-
        /// <summary>
        /// Export report file
        /// </summary>
        private void ExportFile(CrystalDecisions.CrystalReports.Engine.ReportDocument obj)
        {
            ExportOptions CrExportOptions = new ExportOptions();
            DiskFileDestinationOptions CrDiskFileDestinationOptions = new DiskFileDestinationOptions();
            // Create folder to store report file (pdf file)
            string folderName = Directory.GetCurrentDirectory() + "\\PDF File\\" + ReportModel.Code;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            exportFile = folderName + "\\" + DateTime.Now.ToString("yyyy-MM-dd hhmmss") + ".pdf";
            CrDiskFileDestinationOptions.DiskFileName = exportFile;
            PdfRtfWordFormatOptions CrFormatTypeOptions = new PdfRtfWordFormatOptions();
            CrExportOptions = obj.ExportOptions;
            CrExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
            CrExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
            CrExportOptions.DestinationOptions = CrDiskFileDestinationOptions;
            CrExportOptions.FormatOptions = CrFormatTypeOptions;
            obj.Export();
            if (!string.IsNullOrWhiteSpace(ReportModel.CCReport))
            {
                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += new DoWorkEventHandler(sendEmail_DoWork);
                bg.RunWorkerAsync();
            }
        }
        #endregion

        #region -Get group report position by id
        /// <summary>
        /// Get Group report position by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetGroupReportPosition(int id)
        {
            if (GroupReportModelCollection != null)
            {
                int count = GroupReportModelCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    if (GroupReportModelCollection[i].Id == id)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        #endregion

        #region -Set visibility control-
        /// <summary>
        /// -1. Show Main Grid
        /// 0. Show Edit report
        /// 1. Show Set copy
        /// 2. Show Print property
        /// 3. Show Change print
        /// </summary>
        /// <param name="showForm"></param>
        private void SetVisibility(int showForm)
        {
            if (showForm == -1)
            {
                IsShowMainGrid = "Visible";
                IsShowEditReport = "Collapsed";
                IsShowChangePrintProperty = "Collapsed";
                IsShowPrintProperty = "Collapsed";
                IsShowSetCopy = "Collapsed";
                IsShowPrintButtonGroup = "Collapsed";
                IsShowSampleReport = "Collapsed";
            }
            // Show Edit report           
            else
            {
                if (showForm == 0)
                {
                    IsShowChangePrintProperty = "Collapsed";
                    IsShowPrintProperty = "Collapsed";
                    IsShowSetCopy = "Collapsed";
                    IsShowPrintButtonGroup = "Collapsed";
                    IsShowSampleReport = "Collapsed";
                    IsShowMainGrid = "Collapsed";
                    IsShowEditReport = "Visible";
                    GetAllPaperSise();
                }
                else
                {
                    IsShowMainGrid = "Collapsed";
                    IsShowEditReport = "Collapsed";
                    IsShowSampleReport = "Visible";
                    // Show Set copy
                    if (showForm == 1)
                    {
                        IsShowSetCopy = "Visible";
                        IsShowChangePrintProperty = "Collapsed";
                        IsShowPrintProperty = "Collapsed";
                        IsShowPrintButtonGroup = "Collapsed";
                    }
                    // Show Print property
                    else
                    {
                        IsShowSetCopy = "Collapsed";
                        if (showForm == 2)
                        {
                            IsShowPrintButtonGroup = "Visible";
                            IsShowChangePrintProperty = "Collapsed";
                            IsShowPrintProperty = "Visible";
                        }
                        // Show Change print
                        else if (showForm == 3)
                        {
                            IsShowPrintButtonGroup = "Collapsed";
                            IsShowChangePrintProperty = "Visible";
                            IsShowPrintProperty = "Collapsed";
                        }
                    }
                }
                Common.PREVIOUS_SCREEN = showForm;
            }
        }
        #endregion

        #region -Set visibility report-
        /// <summary>
        /// Set visibility report (image).
        /// if value is:
        /// - true: show report
        /// - false: show image report
        /// </summary>
        /// <param name="value"> </param>
        private void SetVisibilityReport(bool value)
        {
            IsShowImageReport = (value == true) ? "Collapsed" : "Visible";
            IsShowReport = (value == true) ? "Visible" : "Collapsed";
        }
        #endregion

        #region -Check duplicate report code-
        /// <summary>
        /// Check duplicate report code
        /// Return: 
        ///     true: duplicate report code
        ///     false: not duplicate
        /// </summary>
        /// <param name="code">code to check</param>
        /// <returns></returns>
        private bool CheckDuplicateReportCode(string code)
        {
            try
            {
                DataTable da = dbHelp.ExecuteQuery("sp_check_report_code", "'" + code + "'");
                if (da.Rows[0][0] != DBNull.Value)
                {
                    return bool.Parse(da.Rows[0][0].ToString());
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
        #endregion

        #region -Set TreeView Icon-
        /// <summary>
        /// Set TreeView Icon
        /// 1. Show Open folder icon.
        /// 2. Show Close folder icon.
        /// 3. Show document icon.
        /// 4. Show Ok icon.
        /// </summary>
        /// <param name="type"></param>
        public void SetTreeViewIcon(short type)
        {
            switch (type)
            {
                case 1:
                    // Show Open folder icon                    
                    ReportModel.IsShowDocument = "Collapsed";
                    ReportModel.IsShowCloseFolder = "Visible";
                    ReportModel.IsShowOK = "Collapsed";
                    ReportModel.IsShowOpenFolder = "Visible";
                    break;
                case 2:
                    // Show close folder icon
                    ReportModel.IsShowDocument = "Collapsed";
                    ReportModel.IsShowCloseFolder = "Visible";
                    ReportModel.IsShowOK = "Collapsed";
                    ReportModel.IsShowOpenFolder = "Collapsed";
                    break;
                case 3:
                    // Show document Icon
                    ReportModel.IsShowOpenFolder = "Collapsed";
                    ReportModel.IsShowCloseFolder = "Collapsed";
                    ReportModel.IsShowOK = "Collapsed";
                    ReportModel.IsShowDocument = "Visible";
                    break;
                case 4:
                    // Set new icon
                    ReportModel.IsShowOpenFolder = "Collapsed";
                    ReportModel.IsShowCloseFolder = "Collapsed";
                    ReportModel.IsShowDocument = "Collapsed";
                    ReportModel.IsShowOK = "Visible";
                    break;
            }
        }
        #endregion

        #region -Load treeview report-
        /// <summary>
        /// Load Treeview Report
        /// </summary>
        private void LoadGroupReport()
        {
            try
            {
                CheckPermission(0);
                // Get all Group model collection                
                GetGroupReportCollection();
                if (GroupReportModelCollection.Count != 0)
                {
                    GroupReportModel = GroupReportModelCollection[0];
                    // Get all Report model collection
                    GetAllReportModel();
                    // Load treeview                    
                    LoadTreeView(GroupReportModel.Id);
                }
                CheckPermission(-2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        #endregion

        #region -Load TreeView by group-
        /// <summary>
        /// Load treeview by group
        /// </summary>
        /// <param name="id">groupid</param>        
        private void LoadTreeView(int id)
        {
            int groupId = GetGroupReportPosition(id);
            if (groupId == -1 || GroupReportModelCollection[groupId].RootReportColection != null)
            {
                return;
            }
            // Add root treeview            
            GroupReportModelCollection[groupId].RootReportColection = new ObservableCollection<rpt_ReportModel>(
                ReportModelCollection.Where(p => p.ParentId == 0 && p.GroupId == id));
            foreach (rpt_ReportModel r in GroupReportModelCollection[groupId].RootReportColection)
            {
                // Get all children in each Root collection
                ObservableCollection<rpt_ReportModel> childNodes = new ObservableCollection<rpt_ReportModel>(
                    ReportModelCollection.Where(p => p.ParentId == r.Id && r.IsShow == true));
                // Show close folder icon
                r.IsShowDocument = "Collapsed";
                r.IsShowCloseFolder = "Visible";
                r.IsShowOK = "Collapsed";
                r.IsShowOpenFolder = "Collapsed";
                if (r.Children == null)
                {
                    r.Children = new ObservableCollection<rpt_ReportModel>();
                }
                foreach (rpt_ReportModel rpt in childNodes)
                {
                    rpt.Parent = r;
                    r.Children.Add(rpt);
                    // Add children node in treeview
                    AddChildrenNode(rpt);
                    // Show document icon
                    rpt.IsShowOpenFolder = "Collapsed";
                    rpt.IsShowCloseFolder = "Collapsed";
                    rpt.IsShowDocument = "Visible";
                    rpt.IsShowOK = "Collapsed";
                }
            }
        }
        #endregion

        #region -Add Children Node-
        /// <summary>
        /// Add children node
        /// </summary>
        /// <param name="rpt"></param>
        private void AddChildrenNode(rpt_ReportModel rpt)
        {
            int id = rpt.Id;
            ObservableCollection<rpt_ReportModel> childrens = new ObservableCollection<rpt_ReportModel>(
                ReportModelCollection.Where(i => i.ParentId == id));
            if (rpt.Children == null)
            {
                rpt.Children = new ObservableCollection<rpt_ReportModel>();
            }
            foreach (rpt_ReportModel i in childrens)
            {
                i.Parent = rpt;
                rpt.Children.Add(i);
                AddChildrenNode(i);
                // Show document icon
                i.IsShowOpenFolder = "Collapsed";
                i.IsShowCloseFolder = "Collapsed";
                i.IsShowDocument = "Visible";
                i.IsShowOK = "Collapsed";
            }
        }
        #endregion

        #region -Backup and Restore report model-
        /// <summary>
        /// Backup report model
        /// </summary>
        private void BackupReport()
        {
            if (ReportModel != null)
            {
                ReportStoreModel = new rpt_ReportModel();
                ReportStoreModel.Id = ReportModel.Id;
                ReportStoreModel.GroupId = ReportModel.GroupId;
                ReportStoreModel.ParentId = ReportModel.ParentId;
                ReportStoreModel.Code = ReportModel.Code;
                ReportStoreModel.Name = ReportModel.Name;
                ReportStoreModel.FormatFile = ReportModel.FormatFile;
                ReportStoreModel.IsShow = ReportModel.IsShow;
                ReportStoreModel.PreProcessName = ReportModel.PreProcessName;
                ReportStoreModel.SamplePicture = ReportModel.SamplePicture;
                ReportStoreModel.PrintTimes = ReportModel.PrintTimes;
                ReportStoreModel.LastPrintDate = ReportModel.LastPrintDate;
                ReportStoreModel.LastPrintUser = ReportModel.LastPrintUser;
                ReportStoreModel.ExcelFile = ReportModel.ExcelFile;
                ReportStoreModel.PrinterName = ReportModel.PrinterName;
                ReportStoreModel.PrintCopy = ReportModel.PrintCopy;
                ReportStoreModel.Remark = ReportModel.Remark;
                ReportStoreModel.DateCreated = ReportModel.DateCreated;
                ReportStoreModel.UserCreated = ReportModel.UserCreated;
                ReportStoreModel.DateUpdated = ReportModel.DateUpdated;
                ReportStoreModel.UserUpdated = ReportModel.UserUpdated;
                ReportStoreModel.PaperSize = ReportModel.PaperSize;
                ReportStoreModel.ScreenTimes = ReportModel.ScreenTimes;
                ReportStoreModel.PrepProcessDescription = ReportModel.PrepProcessDescription;
            }
        }

        /// <summary>
        /// Restore report model
        /// </summary>
        private void RestoreReport()
        {
            if (ReportStoreModel != null)
            {
                ReportModel.Id = ReportStoreModel.Id;
                ReportModel.GroupId = ReportStoreModel.GroupId;
                ReportModel.ParentId = ReportStoreModel.ParentId;
                ReportModel.Code = ReportStoreModel.Code;
                ReportModel.Name = ReportStoreModel.Name;
                ReportModel.FormatFile = ReportStoreModel.FormatFile;
                ReportModel.IsShow = ReportStoreModel.IsShow;
                ReportModel.PreProcessName = ReportStoreModel.PreProcessName;
                ReportModel.SamplePicture = ReportStoreModel.SamplePicture;
                ReportModel.PrintTimes = ReportStoreModel.PrintTimes;
                ReportModel.LastPrintDate = ReportStoreModel.LastPrintDate;
                ReportModel.LastPrintUser = ReportStoreModel.LastPrintUser;
                ReportModel.ExcelFile = ReportStoreModel.ExcelFile;
                ReportModel.PrinterName = ReportStoreModel.PrinterName;
                ReportModel.PrintCopy = ReportStoreModel.PrintCopy;
                ReportModel.Remark = ReportStoreModel.Remark;
                ReportModel.DateCreated = ReportStoreModel.DateCreated;
                ReportModel.UserCreated = ReportStoreModel.UserCreated;
                ReportModel.DateUpdated = ReportStoreModel.DateUpdated;
                ReportModel.UserUpdated = ReportStoreModel.UserUpdated;
                ReportModel.PaperSize = ReportStoreModel.PaperSize;
                ReportModel.ScreenTimes = ReportStoreModel.ScreenTimes;
                ReportModel.PrepProcessDescription = ReportStoreModel.PrepProcessDescription;
                ReportModel.IsDirty = false;
                ReportModel.IsNew = false;
            }
        }
        #endregion

        #region -Get all report model collection -
        /// <summary>
        /// Get all Report Model collection
        /// </summary>
        private void GetAllReportModel()
        {
            if (AllReportModelCollection == null)
            {
                AllReportModelCollection = new ObservableCollection<rpt_ReportModel>(
                    reportRepo.GetAll()
                    .Select(r => new rpt_ReportModel(r))
                    .OrderBy(o => o.GroupId)
                    .ThenBy(x => x.Code)
                    );
                ReportModelCollection = new ObservableCollection<rpt_ReportModel>(
                    AllReportModelCollection.Where(w => w.IsShow == true)
                    .OrderBy(o => o.GroupId)
                    .ThenBy(x => x.Code)
                    );
                lstHiddenReport = AllReportModelCollection.Where(w => !w.IsShow).ToList();
                HiddenReportModelCollection = new ObservableCollection<rpt_ReportModel>(lstHiddenReport);
            }
        }
        #endregion

        #region -Get all group report model-
        /// <summary>
        /// Get all group report model
        /// </summary>
        private void GetGroupReportCollection()
        {
            if (lstGroupReportModel == null)
            {
                // Get all Group model collection
                lstGroupReportModel = new ObservableCollection<rpt_GroupModel>(
                    groupRepo.GetAll().Select(g => new rpt_GroupModel(g))
                    .OrderBy(o => o.Id)
                    ).ToList();
            }
            if (!Common.IS_ADMIN)
            {
                GroupReportModelCollection = new ObservableCollection<rpt_GroupModel>(
                        lstGroupReportModel.Where(w => Common.LST_GROUP.Contains(w.Code)));
            }
            else
            {
                GroupReportModelCollection = new ObservableCollection<rpt_GroupModel>(lstGroupReportModel);
            }
        }
        #endregion

        #region -Get all paper size-
        /// <summary>
        /// Get all paper size
        /// </summary>
        private void GetAllPaperSise()
        {
            if (PaperSizeCollection == null)
            {
                PaperSizeCollection = xmlHelper.GetAllPaperSize();
            }
        }
        #endregion

        #region -Get all pager name-
        /// <summary>
        /// Get paper name
        /// </summary>
        private void GetPaperName()
        {
            foreach (ComboItem pageSize in PaperSizeCollection)
            {
                if (pageSize.Value == ReportModel.PaperSize)
                {
                    ReportModel.PaperName = pageSize.Text;
                    break;
                }
            }
        }
        #endregion

        #region -Get parent report by group-
        /// <summary>
        /// Get parent report by group
        /// </summary>
        /// <param name="id"></param>
        private void GetParentReport(int id)
        {
            // Parent report
            rpt_ReportModel rpt = new rpt_ReportModel();
            rpt.Id = 0;
            rpt.Name = string.Empty;
            rpt.GroupId = id;
            rpt.ParentId = 0;
            rpt.Code = "0";
            rpt.EndUpdate();
            if (ReportModelCollection == null)
            {
                GetAllReportModel();
            }
            ReportModelCollection.Add(rpt);
            // Get all parent each group
            ParentReportModelCollection = new ObservableCollection<rpt_ReportModel>(
                ReportModelCollection.Where(p => (p.ParentId == 0 || p.Id == p.ParentId) && p.GroupId == id).OrderBy(o => o.Name));
            ReportModelCollection.Remove(rpt);
            CurrentParentReportModelCollection = new ObservableCollection<rpt_ReportModel>(ParentReportModelCollection);
        }
        #endregion

        #region -Get Store name-
        /// <summary>
        /// Get Store name by id
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns>return store name</returns>
        private string GetStoreName(int storeCode)
        {
            if (StoreModelCollection == null)
            {
                // Get all store
                GetAllSore();
            }
            if (storeCode >= 0 && storeCode <= StoreModelCollection.Count)
            {
                return StoreModelCollection[storeCode].Name;
            }
            return string.Empty;
        }
        #endregion

        #region -Get all Store-
        /// <summary>
        /// Get Get all Store
        /// </summary>
        private void GetAllSore()
        {
            // Get all Store
            StoreModelCollection = new ObservableCollection<base_StoreModel>(
                storeRepo.GetAll()
                .Select(s => new base_StoreModel(s))
                .OrderBy(o => o.Id)
                );
        }
        #endregion

        #region -Check is valid printer-
        /// <summary>
        /// Check is exist printer name in current computer
        /// </summary>
        /// <param name="printerName"></param>
        /// <returns></returns>
        private string CheckPrinter(string printerName)
        {
            int count = InstalledPrinters.Count;
            for (int i = 0; i < count; i++)
            {
                if (InstalledPrinters[i] == printerName)
                {
                    return printerName;
                }
            }
            return string.Empty;
        }

        #endregion

        #region -Save and update report-
        /// <summary>
        /// Save Report to database
        /// </summary>
        private void SaveReport(int groupID)
        {
            // Set time create report
            ReportModel.DateCreated = DateTime.Now;
            // User create report
            ReportModel.UserCreated = Common.LOGIN_NAME;
            ReportModel.GroupId = GroupReportModel.Id;
            ReportModel.IsShow = true;
            ReportModel.PrintTimes = 0;
            // To entity
            ReportModel.ToEntity();
            reportRepo.Add(ReportModel.rpt_Report);
            // Save changes
            reportRepo.Commit();
            ReportModel.ToModel();
            rpt_ReportModel parent = null;
            if (GroupReportModelCollection[groupID].RootReportColection != null)
            {
                parent = GroupReportModelCollection[groupID].RootReportColection.FirstOrDefault(x => x.Id == ReportModel.ParentId);
            }
            else
            {
                GroupReportModelCollection[groupID].RootReportColection = new ObservableCollection<rpt_ReportModel>();
            }
            if (parent != null)
            {
                ReportModel.Parent = parent;
                parent.Children.Add(ReportModel);
            }
            else
            {
                // Get parent report
                ReportModel.Children = new ObservableCollection<rpt_ReportModel>();
                CurrentParentReportModelCollection.Add(ReportModel);
                GroupReportModelCollection[groupID].RootReportColection.Add(ReportModel);
            }
        }
        /// <summary>
        /// Update report
        /// </summary>
        private void UpdateReport(int groupID)
        {
            rpt_ReportModel newReport = ReportModel;
            int oldParentId = newReport.rpt_Report.ParentId;
            // Set Time update report
            newReport.DateUpdated = DateTime.Now;
            // User update report
            newReport.UserUpdated = Common.LOGIN_NAME;
            newReport.ToEntity();
            // Save changes
            reportRepo.Commit();
            // Reset RelationShip in treeview
            if (oldParentId != newReport.ParentId)
            {
                rpt_ReportModel newParent = GroupReportModelCollection[groupID].RootReportColection.FirstOrDefault(x => x.Id == newReport.ParentId);
                if (newParent != null)
                {
                    newReport.Parent = newParent;
                    newParent.Children.Add(newReport);
                }
                else
                {
                    newReport.Parent = null;
                    GroupReportModelCollection[groupID].RootReportColection.Add(newReport);
                    //GroupReportModelCollection[GroupReportModel.Id - 1].RootReportColection.Add(newReport);
                    // Get parent report 
                    CurrentParentReportModelCollection.Add(newReport);
                }

                rpt_ReportModel oldParent = GroupReportModelCollection[groupID].RootReportColection.FirstOrDefault(x => x.Id == oldParentId);
                if (oldParent != null)
                {
                    int parentId = ReportModel.ParentId;
                    oldParent.Children.Remove(ReportModel);
                    newReport.ParentId = parentId;
                }
            }
        }
        #endregion

        #region -Set is Show Hidden report-
        /// <summary>
        /// Set is show hidden report or main window
        /// if true then Show Hidden report
        /// else show Main Grid
        /// </summary>
        /// <param name="val"></param>
        private void SetIsShowHiddenReport(bool val)
        {
            IsShowMainScreen = (val == true) ? "Collapsed" : "Visible";
            IsShowHiddenReport = (val == true) ? "Visible" : "Collapsed";
        }
        #endregion

        #region -Refresh Repository-
        /// <summary>
        /// Refresh all repository
        /// To Update database after login
        /// </summary>
        private void RefreshRepository()
        {
            storeRepo.Refresh();
            reportRepo.Refresh();
            permissionRepo.Refresh();
        }
        #endregion

        #region -Convert Image to byte Array-

        private void ConvertImageToByteArray()
        {
            try
            {
                string[] a = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceNames();
                string path = "CPC.POSReport.Image.TrueFalseImgs.True.jpg";
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

        #region -Permission-
        /// <summary>
        /// Check user permission
        /// </summary>
        /// <param name="type"></param>
        private void CheckPermission(int type)
        {
            if (Common.IS_ADMIN)
            {
                return;
            }
            da = dbHelp.ExecuteQuery("sp_get_user_permission", string.Format("{0},{1}", Common.USER_RESOURCE, type));
            switch (type)
            {
                case -1:
                    // Set group permission
                    SetGroupPermission();
                    // Check Menu permission
                    SetMenuPermission();
                    // Check Report Permssion
                    SetReportPermission();
                    break;
                case -2:
                    // Check Menu permission
                    SetMenuPermission();
                    // Check Report Permssion
                    SetReportPermission();
                    break;
                case 0:
                    // Set group permission
                    SetGroupPermission();
                    // Check Menu permission
                    break;
                case 1:
                    // Check Report Permssion
                    SetReportPermission();
                    break;
                case 2:
                    // Check Menu permission
                    SetMenuPermission();
                    break;
            }
            da.Clear();
        }

        private void SetGroupPermission()
        {
            Common.LST_GROUP = new List<string>();
            int rowCount = da.Rows.Count;
            // Check Group permission
            for (int i = 0; i < rowCount; i++)
            {
                if (int.Parse(da.Rows[i][0].ToString()) == 0 && (bool)da.Rows[i][4])
                {
                    Common.LST_GROUP.Add(da.Rows[i][1].ToString());
                }
            }
        }

        private void SetMenuPermission()
        {
            int rowCount = da.Rows.Count;
            for (int i = 0; i < rowCount; i++)
            {
                if (int.Parse(da.Rows[i][0].ToString()) != 2 || da.Rows[i][1] == DBNull.Value)
                {
                    continue;
                }
                switch (da.Rows[i][1].ToString())
                {
                    case "M01":
                        Common.SET_PRINT_COPY = (bool)da.Rows[i][4];
                        break;
                    case "M02":
                        Common.PRINT_REPORT = (bool)da.Rows[i][4];
                        break;
                    case "M03":
                        Common.PREVIEW_REPORT = (bool)da.Rows[i][4];
                        break;
                    case "M04":
                        Common.VIEW_SET_COPY = (bool)da.Rows[i][4];
                        break;
                    case "M05":
                        Common.NEW_SET_COPY = (bool)da.Rows[i][4];
                        break;
                    case "M06":
                        Common.DELETE_SET_COPY = (bool)da.Rows[i][4];
                        break;
                    case "M11":
                        Common.ADD_REPORT = (bool)da.Rows[i][4];
                        break;
                    case "M12":
                        Common.EDIT_REPORT = (bool)da.Rows[i][4];
                        break;
                    case "M13":
                        Common.DELETE_REPORT = (bool)da.Rows[i][4];
                        break;
                    case "M14":
                        Common.CHANGE_GROUP_REPORT = (bool)da.Rows[i][4];
                        break;
                    case "M15":
                        Common.NO_SHOW_REPORT = (bool)da.Rows[i][4];
                        break;
                    case "M16":
                        Common.SET_ASSIGN_AUTHORIZE_REPORT = (bool)da.Rows[i][4];
                        break;
                    case "M17":
                        Common.SET_PERMISSION = (bool)da.Rows[i][4];
                        break;
                }
            }
        }

        private void SetReportPermission()
        {
            if (ReportModelCollection == null)
            {
                return;
            }
            int rowCount = da.Rows.Count;
            for (int i = 0; i < rowCount; i++)
            {
                if (int.Parse(da.Rows[i][0].ToString()) != 1)
                {
                    continue;
                }
                for (int k = 0; k < ReportModelCollection.Count; k++)
                {
                    if (da.Rows[i][1].ToString() == ReportModelCollection[k].Code)
                    {
                        ReportModelCollection[k].IsView = (bool)da.Rows[i][2];
                        ReportModelCollection[k].IsPrint = (bool)da.Rows[i][3];
                    }
                }
            }
        }

        #endregion

        #region -Check User Report permission-
        /// <summary>
        /// Check Can View Report 
        /// </summary>
        /// <returns>Return true if can view report else can not view </returns>
        private bool CheckViewable()
        {
            if (Common.IS_ADMIN)
            {
                return true;
            }
            else if (!Common.PREVIEW_REPORT)
            {
                return false;
            }
            return ReportModel.IsView;
        }

        /// <summary>
        /// Check Can Print Report 
        /// </summary>
        /// <returns>Return true if can print report else can not print </returns>
        private bool CheckPrintable()
        {
            if (Common.IS_ADMIN)
            {
                Common.SHOW_PRINT_BUTTON = true;
                return true;
            }
            if (Common.PRINT_REPORT && Common.PREVIEW_REPORT)
            {
                bool isPrint = ReportModel.IsView ? ReportModel.IsPrint : false;
                Common.SHOW_PRINT_BUTTON = isPrint;
                return isPrint;
            }
            return false;
        }
        #endregion

        #region -Update CC Report-
        /// <summary>
        /// update CC Report
        /// </summary>
        /// <param name="ccReport">CC email list</param>
        public void UpdateCCReport(string ccReport)
        {
            try
            {
                ReportModel.CCReport = ccReport;
                ReportModel.ToEntity();
                reportRepo.Commit();
                ReportModel.EndUpdate();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region -Send email to customer-
        /// <summary>
        /// Send email to customer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendEmail_DoWork(object sender, DoWorkEventArgs e)
        {
            da = dbHelp.ExecuteQuery("v_get_email_config");
            if (da.Rows.Count <= 0 || da.Rows[0][0] == DBNull.Value || da.Rows[0][1] == DBNull.Value || da.Rows[0][2] == DBNull.Value || da.Rows[0][3] == DBNull.Value)
            {
                return;
            }
            Common.POP3_EMAIL_SERVER = da.Rows[0][0].ToString();
            Common.POP3_PORT_SERVER = int.Parse(da.Rows[0][1].ToString());
            Common.EMAIL_ACCOUNT = Properties.Settings.Default.EmailAccount;// da.Rows[0][2].ToString();
            Common.EMAIL_PWD = Properties.Settings.Default.EmailPwd;// da.Rows[0][3].ToString();
            da.Clear();
            string errorList = Common.SendEmail(ReportModel.CCReport, ReportModel.Name, exportFile);
        }
        #endregion
        #endregion -private method-
    }
}
