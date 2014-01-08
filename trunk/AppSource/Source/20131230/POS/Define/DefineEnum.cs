using System.ComponentModel;

namespace CPC.POS
{
    // General
    #region SearchType
    public enum SearchType
    {
        None = 0,
        Text = 1,
        Status = 2,
        Numeric = 4,
        Currency = 8,
        Percent = 16,
        Date = 32
    }
    #endregion

    #region ConfigTab
    public enum ConfigTab
    {
        None = 0,
        StoreInfo = 1,
        DocumentNumber = 2,
        DropList = 3,
        CustomField = 4,
        TimeClock = 5
    }
    #endregion

    #region DropListTab
    public enum DropListTab
    {
        None = 0,
        General = 1,
        TimeClock = 2
    }
    #endregion

    #region DropListGroup

    public enum DropListGroup
    {
        None = 0,
        JobTitle = 1,
        Department = 2
    }

    #endregion

    // TimeClock

    #region Overtime
    public enum Overtime
    {
        None = 0,
        Before = 1,
        Break = 2,
        After = 4,
        Holiday = 8
    }
    #endregion

    #region WorkPermissionFilter

    public enum WorkPermissionFilter
    {
        None = 0,
        IncidentsWithoutPermission = 1,
        IncidentsWithPermission = 2,
        Both = IncidentsWithoutPermission | IncidentsWithPermission
    }

    #endregion

    #region WorkPermissionTypes
    public enum WorkPermissionTypes
    {
        None = 0,
        ArrivingLate = 1,
        LeavingEarly = 2,
        Absence = 4,
        SickLeave = 8,
        Vacations = 16,
        DisciplinaryLeave = 32,
        BeforeClockInHour = 64,
        BreakTime = 128,
        AfterClockOutHour = 256,
        HolidayOrDayOff = 512,
        All = ArrivingLate | LeavingEarly | Absence | SickLeave | Vacations | DisciplinaryLeave | BeforeClockInHour | AfterClockOutHour | BreakTime | HolidayOrDayOff
    }
    #endregion

    #region ScheduleTypes
    public enum ScheduleTypes
    {
        Fixed = 1,
        Variables = 2,
        Rotate = 3
    }
    #endregion

    #region ScheduleStatuses
    public enum ScheduleStatuses
    {
        Inactive = 0,
        Active = 1,
        Pending = 2
    }
    #endregion

    #region EmployeeStatuses
    public enum EmployeeStatuses
    {
        Inactive = 0,
        Active = 1,
        Pending = 2
    }
    #endregion

    #region TimelogTypes
    public enum TimelogTypes
    {
        DayOff = 0,
        Attendance = 1,
        Holiday = 2,
        Absence = 3,
        Vacations = 4,
        AbsencePermission = 5,
        AbsencePermissionPaid = 6,
        DisciplinaryLeave = 7,
        SickLeave = 8,
        SickLeavePaid = 9
    }
    #endregion

    #region EmployeeScheduleStatuses
    public enum EmployeeScheduleStatuses
    {
        Inactive = 0,
        Active = 1,
        Pending = 2
    }
    #endregion

    #region HolidayOption

    public enum HolidayOption
    {
        SpecificDay = 0,
        DynamicDay = 1,
        Duration = 2
    }
    #endregion

    #region WeeksOfMonth
    public enum WeeksOfMonth
    {
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Last = 5
    }
    #endregion

    #region DaysOfWeek
    public enum DaysOfWeek
    {
        Day = 1,
        WeekDay = 2,
        WeekendDay = 3,
        Sunday = 4,
        Monday = 5,
        Tuesday = 6,
        Wednesday = 7,
        Thursday = 8,
        Friday = 9,
        Saturday = 10
    }
    #endregion

    #region MonthOfYear

    public enum MonthOfYear
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }

    #endregion

    // TimeClock Report

    #region AnalysisReports
    public enum AnalysisReports
    {
        None = 0,
        GeneralAttendance = 1,
        Absences = 2,
        LateArrivalAndPrematureClockOuts = 3,
        AttendanceSummary = 4,
        LateInstanceSummary = 5,
        LeaveEarlyInstanceSummary = 6,
        PermissionSummary = 7,
        TimeWorkedCountingAbsence = 8,
        TimeWorkedNotCountingAbsence = 9
    }
    #endregion

    // User

    #region PermissionTypes
    public enum PermissionTypes
    {
        None = 0,
        View = 1, // View
        Write = 2, // Save
        Modify = 4, // Update
        Delete = 8, // Remove
        Create = 16, // New
        Print = 32,
        Export = 64, // Excel, Word
        All = View | Write | Modify | Delete | Create | Print | Export
    }
    #endregion

    #region FunctionTypes
    public enum FunctionTypes
    {
        None = 0,
        Employee = 1,
        CreateEmployee = 2,
        EmployeeList = 3,
        TimeClock = 4,
        HolidayAndSchedule = 5,
        Schedule = 6,
        Holiday = 7,
        WorkPermission = 8,
        ManualEditingTimelog = 9,
        CreateWorkPermission = 10,
        WorkPermissionList = 11,
        Report = 12,
        PunctualityComparative = 13,
        Analysis = 14,
        Configuration = 15,
        User = 16,
        GroupPermission = 17,
        CreateGroup = 18,
        GroupList = 19,
        Settings = 20,
        Archive = 21,
        CompanySetting = 22
    }
    #endregion

    // Product

    #region ItemTypes
    public enum ItemTypes
    {
        Stockable = 1,
        NonStocked = 2,
        Services = 3,
        Component = 4,
        Group = 5,
        Insurance = 6
    }
    #endregion

    #region ProductCommissionTypes
    public enum ProductCommissionTypes
    {
        None = 0,
        Percent = 1,
        Money = 2
    }
    #endregion

    #region LengthOfInsuranceTypes
    public enum LengthOfInsuranceTypes
    {
        None = 0,
        Days = 1,
        Weeks = 2,
        Months = 3,
        Years = 4
    }
    #endregion

    #region WarrantyTypes
    public enum WarrantyTypes
    {
        None = 0,
        Manufactory = 1,
        Extention = 2
    }
    #endregion

    #region WarrantyPeriodTypes
    public enum WarrantyPeriodTypes
    {
        None = 0,
        Days = 1,
        Weeks = 2,
        Months = 3,
        Years = 4
    }
    #endregion

    #region ProductStatuses
    public enum ProductStatuses
    {
        Inactive = 0,
        Active = 1
    }
    #endregion

    // Sales Tax
    #region SalesTaxStatuses
    public enum SalesTaxStatuses
    {
        Inactive = 0,
        Active = 1
    }
    #endregion

    //ProductPrice
    #region ProductPrice
    /// <summary>
    /// Kyan Edited 11/04/2013 follow xml
    ///<para>None =0</para>
    ///<para>RegularPrice =1</para>
    ///<para>SalePrice =2</para>
    ///<para>WholesalePrice =4</para>
    ///<para>Employee =8</para>
    ///<para>CustomPrice =16</para>
    /// </summary>

    public enum PriceTypes
    {
        None = 0,
        RegularPrice = 1,
        SalePrice = 2,
        WholesalePrice = 4,
        Employee = 8,
        CustomPrice = 16
    }
    #endregion

    //Edit 07/2/2013

    #region Mark Type
    /// <summary>
    /// Mark Type Using for GuestModel/Purchase/SO/SaleOrderReturn
    /// </summary>
    public enum MarkType
    {
        [Description("E")]
        Employee = 1,
        [Description("C")]
        Customer = 2,
        [Description("V")]
        Vendor = 3,
        [Description("O")]
        Contact = 4,
        [Description("SO")]
        SaleOrder = 5,
        [Description("PO")]
        PurchaseOrder = 6,
        [Description("SR")]
        SaleOrderReturn = 7,
        [Description("LW")]
        Layaway = 8,
        [Description("QO")]
        Quotation = 9,
        [Description("WO")]
        WorkOrder = 10,
        [Description("I")]
        Inventory = 11,
        [Description("S")]
        Shift = 12
    }
    #endregion

    #region CustomerType
    public enum CustomerTypes
    {
        Individual = 1,
        Retailer = 2
    }
    #endregion

    // StatusBasic
    #region StatusBasic
    public enum StatusBasic
    {
        Active = 1,
        Deactive = 2
    }
    #endregion

    public enum TaxInfoType
    {
        FedTaxID = 0,
        TaxLocation = 1,
        ResellerTaxNo = 2,
        TaxExemption = 3
    }

    public enum PriceLevelType
    {
        NoDiscount = 0,
        FixedDiscountOnAllItems = 1,
        MarkdownPriceLevel = 2
    }

    public enum SearchOptions
    {
        None = 0,
        AccountNum = 1,
        FirstName = 2,
        LastName = 4,
        Company = 8,
        Email = 16,
        Phone = 32,
        Code = 64,
        ItemName = 128,
        Category = 256,
        Type = 512,
        PartNumber = 1024,
        Description = 2048,
        Vendor = 4096,
        Barcode = 8192,
        SoNum = 16384,
        Customer = 32768,
        Group = 65536,
        Department,
        Position,
        Status,
        GuestNo,
        City,
        Country,
        CellPhone,
        Website,
        Custom1,
        Custom2,
        Custom3,
        Custom4,
        Custom5,
        Custom6,
        Custom7,
        Custom8,
        Attribute,
        Size,
        ALU,
        UOM,
        TaxCode,
        RegularPrice,
        QuantityOnHand,
        QuantityOnOrder,
        DateCreated,
        UserCreated,
        DateApplied,
        UserApplied,
        DateReversed,
        UserReversed,
        Quantity,
        FromStore,
        ToStore,
        StartDate,
        CompleteDate,
        Counted,
        DateRestored,
        UserRestored,
        AmountChange,
        BasePrice,
        PriceLevel,
        Adjustment,
        ItemCount
    }

    public enum SalesTaxOption
    {
        Single = 0,
        Price = 1,
        Multi = 2
    }

    public enum BarCodeStandar
    {
        [Description("UPC-A")]
        UPCA = 1,
        [Description("UPC-E")]
        UPCE = 1,
        [Description("UPC 2 Digit Ext.")]
        UPC5 = 1,
        [Description("UPC 5 Digit Ext.")]
        EAN13 = 1,
        [Description("EAN-13")]
        JAN13 = 1,
        [Description("JAN-13")]
        EAN8 = 1,
        [Description("EAN-8")]
        ITF14 = 1,
        [Description("ITF-14")]
        CODABAR = 1,
        [Description("Codabar")]
        POSTNET = 1,
        [Description("PostNet")]
        ISBN = 1,
        [Description("Code 11")]
        CODE11 = 1,
        [Description("Code 39")]
        CODE39 = 1,
        [Description("Code 39 Extended")]
        CODE39E = 1,
        [Description("Code 93.")]
        CODE93 = 1,
        [Description("LOGMARS")]
        LOG = 1,
        [Description("MSI")]
        MSI = 1,
        [Description("Interleaved 2 of 5")]
        INTER25 = 1,
        [Description("Standard 2 of 5")]
        STAND25 = 1,
        [Description("Code 12-8")]
        CODE128 = 1,
        [Description("Code 12-8A")]
        CODE128A = 1,
        [Description("Code 12-8B")]
        CODE128B = 1,
        [Description("Code 12-8C")]
        CODE128C = 1,
        [Description("Telepen")]
        TELE = 1,
        [Description("FIM")]
        FIM = 1
    }

    /// <summary>
    /// Used for company setting.
    /// </summary>
    public enum SettingParts
    {
        General,
        StoreInfo,
        Inventory,
        UnitOfMeasure,
        Email,
        MultiStore,
        StoreCodes,
        Pricing,
        Sales,
        Discount,
        Shipping,
        Reward,
        Return,
        PurchaseOrder,
        TimeClock,
        Backup,
        CustomField,
        Voucher
    }

    /// <summary>
    /// Used for popup Department, Category, Brand.
    /// </summary>
    public enum ProductDeparmentLevel
    {
        Department = 0,
        Category = 1,
        Brand = 2
    }

    public enum AddressType
    {
        Home = 0,
        Business = 1,
        Billing = 2,
        Shipping = 3
    }

    /// <summary>
    /// Using for Reward
    /// Money($)/Pecent(%)/Point
    /// </summary>
    public enum RewardType
    {
        Money = 1,
        Pecent = 2,
        Point = 3
    }

    /// <summary>
    /// Using for searching pricing.
    /// </summary>
    public enum SearchPricingOptions
    {
        None = 0,
        Name = 1,
        PriceLevel = 2,
        CouponCode = 4
    }

    /// <summary>
    /// Using for saleorder status
    /// </summary>
    public enum SaleOrderStatus
    {
        Open = 1,
        Shipping = 2,
        FullyShipped = 3,
        Invoiced = 4,
        Cancelled = 5,
        PaidInFull = 6,
        Quote = 7,
        Layaway = 8,
        Close = 9,
        InProcess = 10,
        Void = 11
    }

    /// <summary>
    /// Used for purchase order.
    /// </summary>
    public enum PurchaseStatus
    {
        None = 0,
        Open = 1,
        InProgress = 2,
        FullyReceived = 3,
        PaidInFull = 4,
        Closed = 5
    }

    /// <summary>
    /// Using for searching pricing.
    /// </summary>
    public enum SearchTransferOptions
    {
        None = 0,
        TransferNo = 1,
        DateTransfered = 2
    }

    #region OrderMarkType
    /// <summary>
    /// Mark Type Using for SO and PO
    /// </summary>
    public enum OrderMarkType
    {
        [Description("PO")]
        PurchaseOrder = 1,
        [Description("SO")]
        SaleOrder = 2,
    }
    #endregion

    /// <summary>
    /// Using for searching pricing.
    /// </summary>
    public enum SearchCountSheetOptions
    {
        None = 0,
        SheetNo = 1,
        Status = 2,
        StartedDate = 4,
        CompletedDate = 8,
    }

    public enum GuestRewardStatus
    {
        Available = 1,
        Redeemed = 2,
        Pending = 3,
        Removed = 4
    }

    #region AdjustmentStatus

    /// <summary>
    /// Using for CostAdjustment and QuantityAdjustment
    /// </summary>
    public enum AdjustmentStatus
    {
        Normal = 1,
        Reversing = 2,
        Reversed = 3
    }

    #endregion

    #region AdjustmentReason

    /// <summary>
    /// Using for CostAdjustment and QuantityAdjustment
    /// </summary>
    public enum AdjustmentReason
    {
        ItemEdited = 1,
        Reverse = 2,
        CountedStock = 3,
        TransferedStock = 4
    }

    #endregion

    #region StockStatus

    public enum StockStatus
    {
        Available = 1,
        OnHand = 2,
        OnReserved = 3
    }

    #endregion

    #region ChartType

    public enum ChartType
    {
        ColumnChart = 1,
        BarChart = 2,
        StackedColumnChart = 3,
        StackedBarChart = 4,
        PieChart = 5,
        DoughnutChart = 6
    }

    #endregion

    #region ProductOrderBy

    public enum ProductOrderBy
    {
        TotalProfit = 1,
        OnHandQuantity = 2,
        SaleProfit = 3,
        SoldQuantity = 4,
        QuantityAvailable = 5,
        PurchasedSubTotal = 6,
        PurchasedQuantity = 7
    }

    #endregion

    #region CategoryOrderBy

    public enum CategoryOrderBy
    {
        TotalSale = 1,
        SoldQuantity = 2
    }

    #endregion

    #region OrderDirection

    public enum OrderDirection
    {
        Highest = 1,
        Lowest = 2
    }

    #endregion

    #region MemberShipType
    /// <summary>
    /// Member Ship Customer Reward
    /// </summary>
    public enum MemberShipType
    {
        [Description("P")]
        Platium = 1,
        [Description("G")]
        Gold = 2,
        [Description("S")]
        Silver = 3,
        [Description("B")]
        Bronze = 4,
        [Description("N")]
        Normal = 5
    }
    #endregion

    #region MemberShipStatus
    /// <summary>
    /// Member ship status Customer Reward
    /// </summary>
    public enum MemberShipStatus
    {
        Pending = -1,
        InActived = 0,
        Actived = 1
    }
    #endregion

    public enum PaymentMethod
    {
        [Description("Cash")]
        Cash = 1,
        [Description("Cheque")]
        Cheque = 2,
        [Description("Credit Card")]
        CreditCard = 4,
        [Description("Debit / ATM Card")]
        DebitATMCard = 8,
        [Description("Deposit")]
        Deposit = 16,
        [Description("Account")]
        Account = 32,
        [Description("Gift Card")]
        GiftCard = 64,
        [Description("Gift Certification")]
        GiftCertificate = 128
    }

    public enum ReminderPriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum ReminderRepeat
    {
        Once,
        Daily,
        Weekly,
        Monthly
    }

    #region Skins

    public enum Skins
    {
        Blue = 1,
        Grey = 2,
        Red = 3
    }

    #endregion

    #region BookingChannel

    public enum BookingChannel
    {
        Walkin = 1,
        Website = 2,
        Email = 3,
        Phone = 4
    }

    #endregion

    public enum FolderIn
    {
        [Description("A")]
        Attachment,
        [Description("C")]
        TemplateCategory
    }

    public enum CertificateCardTypeId
    {
        GiftCard = 64,
        GiftCertificate = 128
    }

    public enum GenericCode
    {
        [Description("Job Title")]
        JT = 0
    }

    public enum RewardAmountType
    {
        Cur = 1,
        Per = 2,
        Point = 3
    }

    public enum CutOffScheduleType
    {
        Weekly = 1,
        Monthly = 2,
        Yearly = 3
    }

    public enum CutOffPointType
    {
        Cash = 1,
        Point = 2
    }

    public enum MonthOption
    {
        Day = 1,
        WeeksOfMonth = 2
    }
    public enum YearOption
    {
        Month = 1,
        WeeksOfMonth = 2
    }

    #region WeeklySchedule
    public enum WeeklySchedule
    {
        None = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64
    }
    #endregion

    public enum UnitType
    {
        Percent = 0,
        Money = 1
    }

    public enum CutOffType
    {
        [Description("No Cut-Off")]
        NoCutOff = 1,
        [Description("Date")]
        Date = 2,
        [Description("Cash Or Point")]
        CashOrPoint = 3
    }

}
