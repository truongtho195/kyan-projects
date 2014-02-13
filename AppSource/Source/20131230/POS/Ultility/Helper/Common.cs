using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using CPC.POS;
using CPC.POS.Model;

namespace CPC.Helper
{
    internal partial class Common
    {
        #region Fields

        /// <summary>
        /// Gets 'combo' string.
        /// </summary>
        private const string _comboElement = "combo";

        private static string _contentHappyBirthdayFileName = "ContentHappyBirthday-en-US.txt";
        private static string _rewardContentTemplateFileName = "RewardContentTemplate.txt";

        #endregion

        #region Properties

        #region XMLLanguageFileName

        private static string _XMLLanguageFileName = "en-US.xml";
        /// <summary>
        /// Gets name of language file.
        /// </summary>
        public static string XMLLanguageFileName
        {
            get
            {
                return _XMLLanguageFileName;
            }
            private set
            {
                if (_XMLLanguageFileName != value)
                {
                    _XMLLanguageFileName = value;
                }
            }
        }

        #endregion

        #region LanguageFolder

        private static string _languageFolder = "Language";
        /// <summary>
        /// Gets name of folder that contains language file.
        /// </summary>
        public static string LanguageFolder
        {
            get
            {
                return _languageFolder;
            }
            private set
            {
                if (_languageFolder != value)
                {
                    _languageFolder = value;
                }
            }
        }
        #endregion

        #endregion

        #region Properties Contains Items For ComboBox

        #region EmployeeTypes

        private static IList<ComboItem> _employeeTypes;
        public static IList<ComboItem> EmployeeTypes
        {
            get
            {
                if (null == _employeeTypes)
                    _employeeTypes = GetElements("EmployeeTypes");
                return _employeeTypes;
            }
            private set
            {
                _employeeTypes = value;
            }
        }

        #endregion

        #region EmployeeCommissionTypes

        private static IList<ComboItem> _employeeCommissionTypes;
        public static IList<ComboItem> EmployeeCommissionTypes
        {
            get
            {
                if (null == _employeeCommissionTypes)
                    _employeeCommissionTypes = GetElements("EmployeeCommissionTypes");
                return _employeeCommissionTypes;
            }
            private set
            {
                _employeeCommissionTypes = value;
            }
        }


        #endregion

        #region PayrollTypes

        private static IList<ComboItem> _payrollTypes;
        public static IList<ComboItem> PayrollTypes
        {
            get
            {
                if (null == _payrollTypes)
                    _payrollTypes = GetElements("PayrollTypes");
                return _payrollTypes;
            }
            private set
            {
                _payrollTypes = value;
            }
        }

        #endregion

        #region MaritalStatus

        private static IList<ComboItem> _maritalStatus;
        public static IList<ComboItem> MaritalStatus
        {
            get
            {
                if (null == _maritalStatus)
                    _maritalStatus = GetElements("MaritalStatus", true);
                return _maritalStatus;
            }
            private set
            {
                _maritalStatus = value;
            }
        }

        #endregion

        #region Gender

        private static IList<ComboItem> _gender;
        public static IList<ComboItem> Gender
        {
            get
            {
                if (null == _gender)
                    _gender = GetElements("Gender");
                return _gender;
            }
            private set
            {
                _gender = value;
            }
        }

        #endregion

        #region ScheduleTypes

        private static IList<ComboItem> _scheduleTypes;
        public static IList<ComboItem> ScheduleTypes
        {
            get
            {
                if (null == _scheduleTypes)
                    _scheduleTypes = GetElements("ScheduleTypes");
                return _scheduleTypes;
            }
            private set
            {
                _scheduleTypes = value;
            }
        }

        #endregion

        #region ScheduleStatuses

        private static IList<ComboItem> _scheduleStatuses;
        public static IList<ComboItem> ScheduleStatuses
        {
            get
            {
                if (null == _scheduleStatuses)
                    _scheduleStatuses = GetElements("ScheduleStatuses");
                return _scheduleStatuses;
            }
            private set
            {
                _scheduleStatuses = value;
            }
        }

        #endregion

        #region Countries

        private static IList<ComboItem> _countries;
        public static IList<ComboItem> Countries
        {
            get
            {
                if (null == _countries)
                    _countries = GetElements("country", true);
                return _countries;
            }
            private set
            {
                _countries = value;
            }
        }

        #endregion

        #region States

        private static IList<ComboItem> _states;
        public static IList<ComboItem> States
        {
            get
            {
                if (null == _states)
                    _states = GetElements("state", true);
                return _states;
            }
            private set
            {
                _states = value;
            }
        }

        #endregion

        #region ItemTypes

        private static IList<ComboItem> _itemTypes;
        public static IList<ComboItem> ItemTypes
        {
            get
            {
                if (null == _itemTypes)
                    _itemTypes = GetElements("ItemTypes", true);
                return _itemTypes;
            }
            private set
            {
                _itemTypes = value;
            }
        }

        #endregion

        #region MemberTypes

        private static IList<ComboItem> _memberTypes;
        public static IList<ComboItem> MemberTypes
        {
            get
            {
                if (null == _memberTypes)
                    _memberTypes = GetElements("MemberType");
                return _memberTypes;
            }
            private set
            {
                _memberTypes = value;
            }
        }

        #endregion

        #region PurchaseStatus

        private static IList<ComboItem> _purchaseStatus;
        /// <summary>
        /// Gets Purchase Statuses.
        /// </summary>
        public static IList<ComboItem> PurchaseStatus
        {
            get
            {
                if (null == _purchaseStatus)
                    _purchaseStatus = GetElements("PurchaseStatus", true);
                return _purchaseStatus;
            }
            private set
            {
                _purchaseStatus = value;
            }
        }

        #endregion

        #region WeeksOfMonth

        private static IList<ComboItem> _weeksOfMonth;
        public static IList<ComboItem> WeeksOfMonth
        {
            get
            {
                if (null == _weeksOfMonth)
                    _weeksOfMonth = GetElements("WeeksOfMonth");
                return _weeksOfMonth;
            }
            private set
            {
                _weeksOfMonth = value;
            }
        }

        #endregion

        #region DaysOfWeek

        private static IList<ComboItem> _daysOfWeek;
        public static IList<ComboItem> DaysOfWeek
        {
            get
            {
                if (null == _daysOfWeek)
                    _daysOfWeek = GetElements("DaysOfWeek");
                return _daysOfWeek;
            }
            private set
            {
                _daysOfWeek = value;
            }
        }

        #endregion

        #region Months

        private static IList<ComboItem> _months;
        public static IList<ComboItem> Months
        {
            get
            {
                if (null == _months)
                    _months = GetElements("Months");
                return _months;
            }
            private set
            {
                _months = value;
            }
        }

        #endregion

        #region WorkPermissionType

        private static IList<ComboItem> _workPermissionTypes;
        public static IList<ComboItem> WorkPermissionType
        {
            get
            {
                if (_workPermissionTypes == null)
                    _workPermissionTypes = GetElements("WorkPermissionTypes");
                return _workPermissionTypes;
            }
            private set
            {
                _workPermissionTypes = value;
            }
        }

        #endregion

        #region WarrantyPeriodTypes

        private static IList<ComboItem> _warrantyPeriodTypes;
        public static IList<ComboItem> WarrantyPeriodTypes
        {
            get
            {
                if (null == _warrantyPeriodTypes)
                    _warrantyPeriodTypes = GetElements("WarrantyPeriodType");
                return _warrantyPeriodTypes;
            }
            private set
            {
                _warrantyPeriodTypes = value;
            }
        }

        #endregion

        #region WarrantyTypes

        private static IList<ComboItem> _warrantyTypes;
        public static IList<ComboItem> WarrantyTypes
        {
            get
            {
                if (null == _warrantyTypes)
                    _warrantyTypes = GetElements("WarrantyType");
                return _warrantyTypes;
            }
            private set
            {
                _warrantyTypes = value;
            }
        }

        #endregion

        #region OvertimeTypes

        private static IList<ComboItem> _overtimeTypes;
        /// <summary>
        /// 
        /// </summary>
        public static IList<ComboItem> OvertimeTypes
        {
            get
            {
                if (_overtimeTypes == null)
                    _overtimeTypes = GetElements("OvertimeTypes");
                return _overtimeTypes;
            }
            private set
            {
                _overtimeTypes = value;
            }
        }

        #endregion

        #region UserTypes

        private static IList<ComboItem> _userTypes;
        /// <summary>
        /// 
        /// </summary>
        public static IList<ComboItem> UserTypes
        {
            get
            {
                if (_userTypes == null)
                    _userTypes = GetElements("UserTypes");
                return _userTypes;
            }
            private set
            {
                _userTypes = value;
            }
        }

        #endregion

        #region PayEvents

        private static IList<ComboItem> _payEvents;
        /// <summary>
        /// 
        /// </summary>
        public static IList<ComboItem> PayEvents
        {
            get
            {
                if (_payEvents == null)
                    _payEvents = GetElements("PayEvents", true);
                return _payEvents;
            }
            private set
            {
                _payEvents = value;
            }
        }

        #endregion

        #region CustomerTypes

        private static IList<ComboItem> _customerTypes;
        /// <summary>
        /// Type of Customer 
        /// 1: Individual
        /// 2: Retailer
        /// </summary>
        /// <remarks>
        /// Create By : Kyan 28/01/2013
        /// </remarks>
        public static IList<ComboItem> CustomerTypes
        {
            get
            {
                if (_customerTypes == null)
                    _customerTypes = GetElements("CustomerType");
                return _customerTypes;
            }
            private set
            {
                _customerTypes = value;
            }
        }

        #endregion

        #region MarkdownPriceLevels

        private static IList<ComboItem> _markdownPriceLevels;
        /// <summary>
        /// Term Collection
        ///<para> 1: Regular Price</para>
        ///<para> 2: Wholesale Price</para>
        ///<para> 3: Custom Price</para>
        ///<para> 4: VIP Price</para>
        /// </summary>
        /// <remarks>
        /// Create By : Kyan 29/01/2013
        /// </remarks>
        public static IList<ComboItem> MarkdownPriceLevels
        {
            get
            {
                if (_markdownPriceLevels == null)
                    _markdownPriceLevels = GetElements("PriceSchemas", true);
                return _markdownPriceLevels;
            }
            private set
            {
                _markdownPriceLevels = value;
            }
        }

        #endregion

        #region StatusBasic

        // Basic Status
        private static IList<ComboItem> _statusBasic;
        /// <summary>
        /// Status for Combobox with foreground & background Color
        /// <para>1: Deactive | BackColor : Red | ForeColor : White</para>
        /// <para>2: Active | BackColor: Green | ForeColor : Black </para>
        /// </summary>
        public static IList<ComboItem> StatusBasic
        {
            get
            {
                if (null == _statusBasic)
                    _statusBasic = GetElements("StatusBasic");
                return _statusBasic;
            }
            private set
            {
                _statusBasic = value;
            }
        }

        #endregion

        #region StatusBasicAll

        private static IList<ComboItem> _statusBasicAll;
        /// <summary>
        /// Status for Combobox with foreground & background Color
        /// <para>1: Deactive | BackColor : Red | ForeColor : White</para>
        /// <para>2: Active | BackColor: Green | ForeColor : Black </para>
        /// </summary>
        public static IList<ComboItem> StatusBasicAll
        {
            get
            {
                if (null == _statusBasicAll)
                    _statusBasicAll = GetElements("StatusBasic", true);
                return _statusBasicAll;
            }
            private set
            {
                _statusBasicAll = value;
            }
        }

        #endregion

        #region JobTitles
        private static IList<ComboItem> _jobTitles;
        /// <summary>
        /// Gets the JobTitles
        /// </summary>
        public static IList<ComboItem> JobTitles
        {
            get
            {
                //if (null == _jobTitles)
                //    _jobTitles = GetElements("JobTitles", true);
                return _jobTitles;
            }
            //private set
            set
            {
                _jobTitles = value;
            }
        }

        #endregion

        #region PaymentCardTypes

        private static IList<ComboItem> _paymentCardTypes;
        /// <summary>
        /// Gets the PaymentCardTypes
        /// </summary>
        public static IList<ComboItem> PaymentCardTypes
        {
            get
            {
                if (null == _paymentCardTypes)
                    _paymentCardTypes = GetElements("PaymentCardType");
                return _paymentCardTypes;
            }
            private set
            {
                _paymentCardTypes = value;
            }
        }

        #endregion

        #region PromotionStatuses

        private static IList<ComboItem> _promotionStatuses;
        /// <summary>
        /// Gets the PromotionStatuses
        /// </summary>
        public static IList<ComboItem> PromotionStatuses
        {
            get
            {
                if (null == _promotionStatuses)
                    _promotionStatuses = GetElements("StatusBasic");
                return _promotionStatuses;
            }
            private set
            {
                _promotionStatuses = value;
            }
        }

        #endregion

        #region PromotionTypes

        private static IList<ComboItem> _promotionTypes;
        /// <summary>
        /// Gets the PromotionTypes
        /// </summary>
        public static IList<ComboItem> PromotionTypes
        {
            get
            {
                if (null == _promotionTypes)
                    _promotionTypes = GetElements("PromotionTypes");
                return _promotionTypes;
            }
            private set
            {
                _promotionTypes = value;
            }
        }

        #endregion

        #region PromotionTypeAll

        private static IList<ComboItem> _promotionTypeAll;
        /// <summary>
        /// Gets the PromotionTypeAll
        /// </summary>
        public static IList<ComboItem> PromotionTypeAll
        {
            get
            {
                if (null == _promotionTypeAll)
                    _promotionTypeAll = GetElements("PromotionTypes", true);
                return _promotionTypeAll;
            }
            private set
            {
                _promotionTypeAll = value;
            }
        }

        #endregion

        #region TakeOffOptions

        private static IList<ComboItem> _takeOffOptions;
        /// <summary>
        /// Gets the TakeOffOptions
        /// </summary>
        public static IList<ComboItem> TakeOffOptions
        {
            get
            {
                if (null == _takeOffOptions)
                    _takeOffOptions = GetElements("TakeOffOptions");
                return _takeOffOptions;
            }
            private set
            {
                _takeOffOptions = value;
            }
        }

        #endregion

        #region PriceSchemas

        private static IList<ComboItem> _priceSchemas;
        /// <summary>
        /// Gets the PriceSchemas
        /// </summary>
        public static IList<ComboItem> PriceSchemas
        {
            get
            {
                if (_priceSchemas == null)
                    _priceSchemas = GetElements("PriceSchemas");
                return _priceSchemas;
            }
            private set
            {
                _priceSchemas = value;
            }
        }

        #endregion

        #region PaymentMethods

        private static IList<ComboItem> _paymentMethods;
        /// <summary>
        /// Gets the PaymentMethods
        /// </summary>
        public static IList<ComboItem> PaymentMethods
        {
            get
            {
                if (_paymentMethods == null)
                    _paymentMethods = GetElements("PaymentMethods", true);
                return _paymentMethods;
            }
            private set
            {
                _paymentMethods = value;
            }
        }

        #endregion

        #region BookingChannel

        private static IList<ComboItem> _bookingChannel;
        /// <summary>
        /// Gets the BookingChannel
        /// </summary>
        public static IList<ComboItem> BookingChannel
        {
            get
            {
                if (_bookingChannel == null)
                    _bookingChannel = GetElements("BookingChannel");
                return _bookingChannel;
            }
            private set
            {
                _bookingChannel = value;
            }
        }

        #endregion

        #region StatusSalesOrders

        private static IList<ComboItem> _statusSalesOrders;
        /// <summary>
        /// Gets the Sales Order Status
        /// </summary>
        public static IList<ComboItem> StatusSalesOrders
        {
            get
            {
                if (_statusSalesOrders == null)
                    _statusSalesOrders = GetElements("SaleStatus");
                return _statusSalesOrders;
            }
            private set
            {
                _statusSalesOrders = value;
            }
        }

        #endregion

        #region Languages

        private static IList<ComboItem> _languages;
        public static IList<ComboItem> Languages
        {
            get
            {
                if (null == _languages)
                    _languages = GetElements("Language");
                return _languages;
            }
            private set
            {
                _languages = value;
            }
        }

        #endregion

        #region Shifts

        private static IList<ComboItem> _shifts;
        public static IList<ComboItem> Shifts
        {
            get
            {
                if (null == _shifts)
                    _shifts = GetElements("Shift");
                return _shifts;
            }
            private set
            {
                _shifts = value;
            }
        }

        #endregion

        #region StockStatus

        private static IList<ComboItem> _stockStatus;
        /// <summary>
        /// Gets or sets the StockStatus
        /// </summary>
        public static IList<ComboItem> StockStatus
        {
            get
            {
                if (_stockStatus == null)
                    _stockStatus = GetElements("StockStatus");
                return _stockStatus;
            }
            private set
            {
                _stockStatus = value;
            }
        }

        #endregion

        #region RewardAmountTypes

        private static IList<ComboItem> _rewardAmountTypes;
        public static IList<ComboItem> RewardAmountTypes
        {
            get
            {
                if (null == _rewardAmountTypes)
                    _rewardAmountTypes = GetElements("RewardAmount", true);
                return _rewardAmountTypes;
            }
            private set
            {
                _rewardAmountTypes = value;
            }
        }

        #endregion

        #region RewardExpirationTypes

        private static IList<ComboItem> _rewardExpirationTypes;
        public static IList<ComboItem> RewardExpirationTypes
        {
            get
            {
                if (null == _rewardExpirationTypes)
                    _rewardExpirationTypes = GetElements("RewardExpiration", true);
                return _rewardExpirationTypes;
            }
            private set
            {
                _rewardExpirationTypes = value;
            }
        }

        #endregion

        #region WarrantyTypeAll

        private static IList<ComboItem> _warrantyTypeAll;
        public static IList<ComboItem> WarrantyTypeAll
        {
            get
            {
                if (null == _warrantyTypeAll)
                    _warrantyTypeAll = GetElements("WarrantyType", true);
                return _warrantyTypeAll;
            }
            private set
            {
                _warrantyTypeAll = value;
            }
        }

        #endregion

        #region WarrantyPeriodTypeAll

        private static IList<ComboItem> _warrantyPeriodTypeAll;
        public static IList<ComboItem> WarrantyPeriodTypeAll
        {
            get
            {
                if (null == _warrantyPeriodTypeAll)
                    _warrantyPeriodTypeAll = GetElements("WarrantyPeriodType", true);
                return _warrantyPeriodTypeAll;
            }
            private set
            {
                _warrantyPeriodTypeAll = value;
            }
        }

        #endregion

        #region BasePriceTypes

        private static IList<ComboItem> _basePriceTypes;
        public static IList<ComboItem> BasePriceTypes
        {
            get
            {
                if (null == _basePriceTypes)
                    _basePriceTypes = GetElements("BasePriceType");
                return _basePriceTypes;
            }
            private set
            {
                _basePriceTypes = value;
            }
        }

        #endregion

        #region AdjustmentTypes

        private static IList<ComboItem> _adjustmentTypes;
        public static IList<ComboItem> AdjustmentTypes
        {
            get
            {
                if (null == _adjustmentTypes)
                    _adjustmentTypes = GetElements("PricingAdjustmentType");
                return _adjustmentTypes;
            }
            private set
            {
                _adjustmentTypes = value;
            }
        }

        #endregion

        #region PricingStatus

        private static IList<ComboItem> _pricingStatus;
        public static IList<ComboItem> PricingStatus
        {
            get
            {
                if (null == _pricingStatus)
                    _pricingStatus = GetElements("PricingStatus");
                return _pricingStatus;
            }
            private set
            {
                _pricingStatus = value;
            }
        }

        #endregion

        #region ShipUnits

        private static IList<ComboItem> _shipUnits;
        public static IList<ComboItem> ShipUnits
        {
            get
            {
                if (null == _shipUnits)
                    _shipUnits = GetElements("ShipUnit");
                return _shipUnits;
            }
            private set
            {
                _shipUnits = value;
            }
        }

        #endregion

        #region TransferStockStatus

        private static IList<ComboItem> _transferStockStatus;
        public static IList<ComboItem> TransferStockStatus
        {
            get
            {
                if (null == _transferStockStatus)
                    _transferStockStatus = GetElements("TransferStockStatus");
                return _transferStockStatus;
            }
            private set
            {
                _transferStockStatus = value;
            }
        }

        #endregion

        #region CountStockStatus

        private static IList<ComboItem> _countStockStatus;
        public static IList<ComboItem> CountStockStatus
        {
            get
            {
                if (null == _countStockStatus)
                    _countStockStatus = GetElements("CountStockStatus");
                return _countStockStatus;
            }
            private set
            {
                _countStockStatus = value;
            }
        }

        #endregion

        #region AvailableQuantities

        private static IList<ComboItem> _availableQuantities;
        public static IList<ComboItem> AvailableQuantities
        {
            get
            {
                if (null == _availableQuantities)
                    _availableQuantities = GetElements("AvailableQuantity");
                return _availableQuantities;
            }
            private set
            {
                _availableQuantities = value;
            }
        }

        #endregion

        #region GuestRewardStatus

        private static IList<ComboItem> _guestRewardStatus;
        public static IList<ComboItem> GuestRewardStatus
        {
            get
            {
                if (null == _guestRewardStatus)
                    _guestRewardStatus = GetElements("RewardStatus");
                return _guestRewardStatus;
            }
            private set
            {
                _guestRewardStatus = value;
            }
        }
        #endregion

        #region AdjustmentStatus

        private static IList<ComboItem> _adjustmentStatus;
        /// <summary>
        /// Gets or sets the AdjustmentStatus
        /// </summary>
        public static IList<ComboItem> AdjustmentStatus
        {
            get
            {
                if (null == _adjustmentStatus)
                    _adjustmentStatus = GetElements("AdjustmentStatus");
                return _adjustmentStatus;
            }
            private set
            {
                _adjustmentStatus = value;
            }
        }

        #endregion

        #region AdjustmentStatusAll

        private static IList<ComboItem> _adjustmentStatusAll;
        /// <summary>
        /// Gets or sets the AdjustmentStatusAll
        /// </summary>
        public static IList<ComboItem> AdjustmentStatusAll
        {
            get
            {
                if (null == _adjustmentStatusAll)
                    _adjustmentStatusAll = GetElements("AdjustmentStatus", true);
                return _adjustmentStatusAll;
            }
            private set
            {
                _adjustmentStatusAll = value;
            }
        }

        #endregion

        #region AdjustmentReason

        private static IList<ComboItem> _adjustmentReason;
        /// <summary>
        /// Gets or sets the AdjustmentReason
        /// </summary>
        public static IList<ComboItem> AdjustmentReason
        {
            get
            {
                if (null == _adjustmentReason)
                    _adjustmentReason = GetElements("AdjustmentReason");
                return _adjustmentReason;
            }
            private set
            {
                _adjustmentReason = value;
            }
        }

        #endregion

        #region AdjustmentReasonAll

        private static IList<ComboItem> _adjustmentReasonAll;
        /// <summary>
        /// Gets or sets the AdjustmentReasonAll
        /// </summary>
        public static IList<ComboItem> AdjustmentReasonAll
        {
            get
            {
                if (null == _adjustmentReasonAll)
                    _adjustmentReasonAll = GetElements("AdjustmentReason", true);
                return _adjustmentReasonAll;
            }
            private set
            {
                _adjustmentReasonAll = value;
            }
        }

        #endregion

        #region ItemSettings

        public static IList<ItemSettingModel> ItemSettings;

        #endregion

        #region Currency

        private static IList<ComboItem> _currency;
        /// <summary>
        /// Gets or sets the Currency
        /// </summary>
        public static IList<ComboItem> Currency
        {
            get
            {
                if (null == _currency)
                    _currency = GetElements("Currency");
                return _currency;
            }
            private set
            {
                _currency = value;
            }
        }

        #endregion

        #region ChartType

        private static IList<ComboItem> _chartType;
        /// <summary>
        /// Gets or sets the ChartType
        /// </summary>
        public static IList<ComboItem> ChartType
        {
            get
            {
                if (null == _chartType)
                    _chartType = GetElements("ChartType");
                return _chartType;
            }
            private set
            {
                _chartType = value;
            }
        }

        #endregion

        #region ProductOrderBy

        private static IList<ComboItem> _productOrderBy;
        /// <summary>
        /// Gets or sets the ProductOrderBy
        /// </summary>
        public static IList<ComboItem> ProductOrderBy
        {
            get
            {
                if (null == _productOrderBy)
                    _productOrderBy = GetElements("ProductOrderBy");
                return _productOrderBy;
            }
            private set
            {
                _productOrderBy = value;
            }
        }

        #endregion

        #region CategoryOrderBy

        private static IList<ComboItem> _categoryOrderBy;
        /// <summary>
        /// Gets or sets the CategoryOrderBy
        /// </summary>
        public static IList<ComboItem> CategoryOrderBy
        {
            get
            {
                if (null == _categoryOrderBy)
                    _categoryOrderBy = GetElements("CategoryOrderBy");
                return _categoryOrderBy;
            }
            private set
            {
                _categoryOrderBy = value;
            }
        }

        #endregion

        #region OrderDirection

        private static IList<ComboItem> _orderDirection;
        /// <summary>
        /// Gets or sets the OrderDirection
        /// </summary>
        public static IList<ComboItem> OrderDirection
        {
            get
            {
                if (null == _orderDirection)
                    _orderDirection = GetElements("OrderDirection");
                return _orderDirection;
            }
            private set
            {
                _orderDirection = value;
            }
        }

        #endregion

        #region GroupRight

        private static IList<ComboItem> _groupRight;
        /// <summary>
        /// Gets or sets the GroupRight
        /// </summary>
        public static IList<ComboItem> GroupRight
        {
            get
            {
                if (null == _groupRight)
                    _groupRight = GetElements("GroupRight", true);
                return _groupRight;
            }
            private set
            {
                _groupRight = value;
            }
        }

        #endregion

        #region AddressTypes

        private static IList<ComboItem> _addressTypes;
        /// <summary>
        /// Gets or sets the GroupRight
        /// </summary>
        public static IList<ComboItem> AddressTypes
        {
            get
            {
                if (null == _addressTypes)
                    _addressTypes = GetElements("AddressTypes", true);
                return _addressTypes;
            }
            private set
            {
                _addressTypes = value;
            }
        }

        #endregion

        #region TextNumberAlignments

        private static IList<ComboItem> _textNumberAlignments;
        /// <summary>
        /// Gets or sets the TextNumberAlignments.
        /// </summary>
        public static IList<ComboItem> TextNumberAlignments
        {
            get
            {
                if (null == _textNumberAlignments)
                    _textNumberAlignments = GetElements("TextNumberAlignment", true);
                return _textNumberAlignments;
            }
            private set
            {
                _textNumberAlignments = value;
            }
        }

        #endregion

        #region Title

        private static IList<ComboItem> _title;
        /// <summary>
        /// Gets or sets the PersonalTitle
        /// </summary>
        public static IList<ComboItem> Title
        {
            get
            {
                if (null == _title)
                    _title = GetElements("Title", true);
                return _title;
            }
            private set
            {
                _title = value;
            }
        }

        #endregion

        #region PriorityList

        private static IList<ComboItem> _priorityList;
        /// <summary>
        /// Gets or sets the PriorityList
        /// </summary>
        public static IList<ComboItem> PriorityList
        {
            get
            {
                if (null == _priorityList)
                    _priorityList = GetElements("Priority", true);
                return _priorityList;
            }
            private set
            {
                _priorityList = value;
            }
        }

        #endregion

        #region ReminderCategories

        private static IList<ComboItem> _reminderCategories;
        /// <summary>
        /// Gets or sets the ReminderCategories
        /// </summary>
        public static IList<ComboItem> ReminderCategories
        {
            get
            {
                if (null == _reminderCategories)
                    _reminderCategories = GetElements("ReminderCategory", true);
                return _reminderCategories;
            }
            private set
            {
                _reminderCategories = value;
            }
        }

        #endregion

        #region ReminderRepeatList

        private static IList<ComboItem> _reminderRepeatList;
        /// <summary>
        /// Gets or sets the ReminderRepeatList
        /// </summary>
        public static IList<ComboItem> ReminderRepeatList
        {
            get
            {
                if (null == _reminderRepeatList)
                    _reminderRepeatList = GetElements("ReminderRepeat", true);
                return _reminderRepeatList;
            }
            private set
            {
                _reminderRepeatList = value;
            }
        }

        #endregion

        #region PaymentSchedule

        private static IList<ComboItem> _paymentSchedule;
        /// <summary>
        /// Gets or sets the GroupRight
        /// </summary>
        public static IList<ComboItem> PaymentSchedule
        {
            get
            {
                if (null == _paymentSchedule)
                    _paymentSchedule = GetElements("PaymentSchedule", true);
                return _paymentSchedule;
            }
            private set
            {
                _paymentSchedule = value;
            }
        }

        #endregion

        #region CashList

        private static IList<ComboItem> _cashList;
        /// <summary>
        /// Gets or sets the CashList
        /// </summary>
        public static IList<ComboItem> CashList
        {
            get
            {
                if (null == _cashList)
                    _cashList = GetElements("CashList", true);
                return _cashList;
            }
            private set
            {
                _cashList = value;
            }
        }

        #endregion

        #region DrawerList

        private static IList<ComboItem> _drawerList;
        /// <summary>
        /// Gets or sets the DrawerList
        /// </summary>
        public static IList<ComboItem> DrawerList
        {
            get
            {
                if (null == _drawerList)
                    _drawerList = GetElements("DrawerList", true);
                return _drawerList;
            }
            private set
            {
                _drawerList = value;
            }
        }

        #endregion

        #region CutOffTypes
        private static IList<ComboItem> _cutOffTypes;
        /// <summary>
        /// Gets or sets the CutOffTypes
        /// </summary>
        public static IList<ComboItem> CutOffTypes
        {
            get
            {
                if (null == _cutOffTypes)
                    _cutOffTypes = GetElements("CutOffType", true);
                return _cutOffTypes;
            }
            private set
            {
                _cutOffTypes = value;
            }
        }
        #endregion

        #region ScanMethods
        private static IList<ComboItem> _scanMethods;
        /// <summary>
        /// Gets or sets the ScanMethods
        /// </summary>
        public static IList<ComboItem> ScanMethods
        {
            get
            {
                if (null == _scanMethods)
                    _scanMethods = GetElements("ScanMethods", true);
                return _scanMethods;
            }
            private set
            {
                _scanMethods = value;
            }
        }
        #endregion

        #region JobTitles
        private static IList<ComboItem> _deparments;
        /// <summary>
        /// Gets the JobTitles
        /// </summary>
        public static IList<ComboItem> Departments
        {
            get
            {
                
                return _deparments;
            }
            //private set
            set
            {
                _deparments = value;
            }
        }

        #endregion

        #endregion

        #region Read Xml Methods

        #region GetElements

        public static IList<ComboItem> GetElements(string key, bool isAll = false)
        {
            IList<ComboItem> comboItems = new List<ComboItem>();
            using (Stream stream = LoadCurrentLanguagePackage())
            {
                if (stream == null)
                {
                    return comboItems;
                }

                XDocument doc = XDocument.Load(stream);

                var query = from p in doc.Root.Elements(_comboElement)
                            where p.Attribute("key").Value == key
                            select p;
                if (null != query)
                {
                    foreach (var item in query.Single().Elements())
                    {
                        ComboItem comboItem = new ComboItem();
                        comboItem.Value = item.Element("value") != null ? Convert.ToInt16(item.Element("value").Value) : (short)0;
                        comboItem.Text = item.Element("name") != null ? item.Element("name").Value : null;
                        comboItem.Code = item.Element("code") != null ? item.Element("code").Value : null;
                        comboItem.IntValue = item.Element("default") != null ? Convert.ToInt32(item.Element("default").Value) : 0;
                        comboItem.ObjValue = item.Element("value") != null ? item.Element("value").Value : null;
                        comboItem.Flag = item.Element("flag") != null ? bool.Parse(item.Element("flag").Value) : false;
                        comboItem.Group = item.Element("group") != null ? item.Element("group").Value : null;
                        comboItem.Symbol = item.Element("symbol") != null ? item.Element("symbol").Value : null;
                        comboItem.ParentId = item.Element("parentId") != null ? Convert.ToInt32(item.Element("parentId").Value) : 0;
                        comboItem.Detail = item.Element("detail") != null ? item.Element("detail").Value : "0";
                        comboItem.CultureInfo = item.Element("culture") != null ? new CultureInfo(item.Element("culture").Value) : null;
                        comboItem.SettingPart = item.Element("settingPart") != null ? item.Element("settingPart").Value : null;
                        comboItem.Tab = item.Element("tab") != null ? Convert.ToInt32(item.Element("tab").Value) : 0;
                        comboItem.Islocked = item.Element("islocked") != null ? bool.Parse(item.Element("islocked").Value) : false;
                        comboItem.HasState = item.Element("hasstate") != null ? bool.Parse(item.Element("hasstate").Value) : false;

                        if ((!isAll && comboItem.Value > 0) || isAll || item.Element("value") == null)
                        {
                            comboItems.Add(comboItem);
                        }
                    }
                }
            }
            return comboItems;
        }

        #endregion

        #region GetElementsOnlyValueText

        /// <summary>
        /// Gets all elements in document only value and text based on name and key of elements.
        /// </summary>
        /// <param name="xDocument">XDocument that contains all elements.</param>
        /// <param name="elementName">Name of element.</param>
        /// <param name="key">Key of element.</param>
        /// <returns></returns>
        public static IList<ComboItem> GetElementsOnlyValueText(XDocument xDocument, string elementName, string key)
        {
            if (xDocument == null)
            {
                throw new ArgumentNullException("xDocument");
            }

            IList<ComboItem> comboItems = new List<ComboItem>();

            if (string.IsNullOrWhiteSpace(elementName))
            {
                return comboItems;
            }

            IEnumerable<XElement> query;
            if (!string.IsNullOrWhiteSpace(key))
            {
                query = from p in xDocument.Root.Elements(elementName)
                        where p.Attribute("key").Value == key
                        select p;
            }
            else
            {
                query = from p in xDocument.Root.Elements(elementName)
                        select p;
            }

            if (null != query && query.Any())
            {
                foreach (var item in query.Single().Elements())
                {
                    ComboItem comboItem = new ComboItem
                    {
                        Value = Convert.ToInt16(item.Element("value").Value),
                        Text = item.Element("name").Value,
                    };

                    comboItems.Add(comboItem);
                }
            }

            return comboItems;
        }

        #endregion

        #endregion

        #region Methods

        #region LoadCurrentLanguagePackage

        public static Stream LoadCurrentLanguagePackage()
        {
            DirectoryInfo directoryExecuting = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            DirectoryInfo dataFolder = directoryExecuting.GetDirectories(LanguageFolder).FirstOrDefault();
            if (dataFolder != null)
            {
                FileInfo templateFileInfo = dataFolder.GetFiles(_contentHappyBirthdayFileName).FirstOrDefault();
                if (templateFileInfo != null)
                {
                    Define.ContentHappyBirthdayFile = templateFileInfo.FullName;
                }

                templateFileInfo = dataFolder.GetFiles(_rewardContentTemplateFileName).FirstOrDefault();
                if (templateFileInfo != null)
                {
                    Define.RewardContentTemplateFile = templateFileInfo.FullName;
                }

                FileInfo languagePackage = dataFolder.GetFiles(XMLLanguageFileName).FirstOrDefault();
                if (languagePackage != null)
                {
                    if (languagePackage.IsReadOnly)
                    {
                        languagePackage.IsReadOnly = false;
                    }
                    return new FileStream(languagePackage.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
            }
            return null;
        }

        #endregion

        #region Refresh

        /// <summary>
        /// Refresh data.
        /// </summary>
        public static void Refresh()
        {
            foreach (PropertyInfo propInfo in typeof(Common).GetProperties())
            {
                if (propInfo.CanWrite && !propInfo.PropertyType.IsValueType && propInfo.PropertyType.Namespace == "System.Collections.Generic")
                {
                    if (!propInfo.Name.Equals("JobTitles"))
                        propInfo.SetValue(null, null, null);
                }
            }
        }

        #endregion

        #region GetPropertyName

        /// <summary>
        /// Get name of property to binding
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            return (propertyExpression.Body as MemberExpression).Member.Name;
        }

        #endregion

        #region ChangeLanguage

        /// <summary>
        /// Change language
        /// </summary>
        /// <param name="cultureInfo">New culture.</param>
        public static void ChangeLanguage(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                return;
            }

            IList<ComboItem> itemList = null;
            ComboItem comboItemTemp = null;
            string oldXAMLLanguageFileName = string.Format("{0}.xaml", Thread.CurrentThread.CurrentUICulture.Name);
            string newXAMLLanguageFileName = string.Format("{0}.xaml", cultureInfo.Name);

            // Changes language.
            ResourceDictionary language = Application.Current.Resources.MergedDictionaries.FirstOrDefault(x =>
                x.Source.ToString().Contains(oldXAMLLanguageFileName));
            language.Source = new Uri(string.Format(@"..\Language\{0}", newXAMLLanguageFileName), UriKind.Relative);

            // If changes language successfull, update CurrentUICulturea and XMLLanguageFileName.
            XMLLanguageFileName = string.Format("{0}.xml", cultureInfo.Name);
            _contentHappyBirthdayFileName = string.Format("ContentHappyBirthday-{0}.txt", cultureInfo.Name);

            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Stream stream = LoadCurrentLanguagePackage();
            if (stream == null)
            {
                return;
            }
            XDocument xDocument = XDocument.Load(stream);
            if (xDocument == null)
            {
                return;
            }

            #region EmployeeTypes

            if (_employeeTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "EmployeeTypes");
                if (itemList != null)
                {
                    foreach (var item in EmployeeTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region EmployeeCommissionTypes

            if (_employeeCommissionTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "EmployeeCommissionTypes");
                if (itemList != null)
                {
                    foreach (var item in EmployeeCommissionTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PayrollTypes

            if (_payrollTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PayrollTypes");
                if (itemList != null)
                {
                    foreach (var item in PayrollTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region MaritalStatus

            if (_maritalStatus != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "MaritalStatus");
                if (itemList != null)
                {
                    foreach (var item in MaritalStatus)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region Gender

            if (_gender != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "Gender");
                if (itemList != null)
                {
                    foreach (var item in Gender)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region ScheduleTypes

            if (_scheduleTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "ScheduleTypes");
                if (itemList != null)
                {
                    foreach (var item in ScheduleTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region ScheduleStatuses

            if (_scheduleStatuses != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "ScheduleStatuses");
                if (itemList != null)
                {
                    foreach (var item in ScheduleStatuses)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region Countries

            if (_countries != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "country");
                if (itemList != null)
                {
                    foreach (var item in Countries)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region States

            if (_states != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "state");
                if (itemList != null)
                {
                    foreach (var item in States)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region ItemTypes

            if (_itemTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "ItemTypes");
                if (itemList != null)
                {
                    foreach (var item in ItemTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region MemberTypes

            if (_memberTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "MemberType");
                if (itemList != null)
                {
                    foreach (var item in MemberTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PurchaseStatus

            if (_purchaseStatus != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PurchaseStatus");
                if (itemList != null)
                {
                    foreach (var item in PurchaseStatus)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region WeeksOfMonth

            if (_weeksOfMonth != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "WeeksOfMonth");
                if (itemList != null)
                {
                    foreach (var item in WeeksOfMonth)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region DaysOfWeek

            if (_daysOfWeek != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "DaysOfWeek");
                if (itemList != null)
                {
                    foreach (var item in DaysOfWeek)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region Months

            if (_months != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "Months");
                if (itemList != null)
                {
                    foreach (var item in Months)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region WorkPermissionType

            if (_workPermissionTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "WorkPermissionTypes");
                if (itemList != null)
                {
                    foreach (var item in WorkPermissionType)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region OvertimeTypes

            if (_overtimeTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "OvertimeTypes");
                if (itemList != null)
                {
                    foreach (var item in OvertimeTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region UserTypes

            if (_userTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "UserTypes");
                if (itemList != null)
                {
                    foreach (var item in UserTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PayEvents

            if (_payEvents != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PayEvents");
                if (itemList != null)
                {
                    foreach (var item in PayEvents)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region CustomerTypes

            if (_customerTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "CustomerType");
                if (itemList != null)
                {
                    foreach (var item in CustomerTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region MarkdownPriceLevels

            if (_markdownPriceLevels != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PriceSchemas");
                if (itemList != null)
                {
                    foreach (var item in MarkdownPriceLevels)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PriceSchemas

            if (_priceSchemas != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PriceSchemas");
                if (itemList != null)
                {
                    foreach (var item in PriceSchemas)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PaymentCardTypes

            if (_paymentCardTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PaymentCardType");
                if (itemList != null)
                {
                    foreach (var item in PaymentCardTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PaymentMethods

            if (_paymentMethods != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PaymentMethods");
                if (itemList != null)
                {
                    foreach (var item in PaymentMethods)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PromotionTypes

            if (_promotionTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PromotionTypes");
                if (itemList != null)
                {
                    foreach (var item in PromotionTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PromotionTypeAll

            if (_promotionTypeAll != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PromotionTypes");
                if (itemList != null)
                {
                    foreach (var item in PromotionTypeAll)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region TakeOffOptions

            if (_takeOffOptions != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "TakeOffOptions");
                if (itemList != null)
                {
                    foreach (var item in TakeOffOptions)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region BookingChannel

            if (_bookingChannel != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "BookingChannel");
                if (itemList != null)
                {
                    foreach (var item in BookingChannel)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region ShipUnits

            if (_shipUnits != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "ShipUnit");
                if (itemList != null)
                {
                    foreach (var item in ShipUnits)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region StatusSalesOrders

            if (_statusSalesOrders != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "SaleStatus");
                if (itemList != null)
                {
                    foreach (var item in StatusSalesOrders)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region AvailableQuantities

            if (_availableQuantities != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "AvailableQuantity");
                if (itemList != null)
                {
                    foreach (var item in AvailableQuantities)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region StockStatus

            if (_stockStatus != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "StockStatus");
                if (itemList != null)
                {
                    foreach (var item in StockStatus)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region Languages

            if (_languages != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "Language");
                if (itemList != null)
                {
                    foreach (var item in Languages)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region Shifts

            if (_shifts != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "Shift");
                if (itemList != null)
                {
                    foreach (var item in Shifts)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region RewardAmountTypes

            if (_rewardAmountTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "RewardAmount");
                if (itemList != null)
                {
                    foreach (var item in RewardAmountTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region GuestRewardStatus

            if (_guestRewardStatus != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "RewardStatus");
                if (itemList != null)
                {
                    foreach (var item in GuestRewardStatus)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region RewardExpirationTypes

            if (_rewardExpirationTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "RewardExpiration");
                if (itemList != null)
                {
                    foreach (var item in RewardExpirationTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region WarrantyTypes

            if (_warrantyTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "WarrantyType");
                if (itemList != null)
                {
                    foreach (var item in WarrantyTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region WarrantyTypeAll

            if (_warrantyTypeAll != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "WarrantyType");
                if (itemList != null)
                {
                    foreach (var item in WarrantyTypeAll)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region WarrantyPeriodTypes

            if (_warrantyPeriodTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "WarrantyPeriodType");
                if (itemList != null)
                {
                    foreach (var item in WarrantyPeriodTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region WarrantyPeriodTypeAll

            if (_warrantyPeriodTypeAll != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "WarrantyPeriodType");
                if (itemList != null)
                {
                    foreach (var item in WarrantyPeriodTypeAll)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region BasePriceTypes

            if (_basePriceTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "BasePriceType");
                if (itemList != null)
                {
                    foreach (var item in BasePriceTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PricingStatus

            if (_pricingStatus != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PricingStatus");
                if (itemList != null)
                {
                    foreach (var item in PricingStatus)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region TransferStockStatus

            if (_transferStockStatus != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "TransferStockStatus");
                if (itemList != null)
                {
                    foreach (var item in TransferStockStatus)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region AdjustmentTypes

            if (_adjustmentTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PricingAdjustmentType");
                if (itemList != null)
                {
                    foreach (var item in AdjustmentTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region CountStockStatus

            if (_countStockStatus != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "CountStockStatus");
                if (itemList != null)
                {
                    foreach (var item in CountStockStatus)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region StatusBasic

            if (_statusBasic != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "StatusBasic");
                if (itemList != null)
                {
                    foreach (var item in StatusBasic)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region StatusBasicAll

            if (_statusBasicAll != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "StatusBasic");
                if (itemList != null)
                {
                    foreach (var item in StatusBasicAll)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PromotionStatuses

            if (_promotionStatuses != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "StatusBasic");
                if (itemList != null)
                {
                    foreach (var item in PromotionStatuses)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region ItemSettings

            if (ItemSettings != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "ItemSetting");
                if (itemList != null)
                {
                    foreach (var item in ItemSettings)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Id);
                        if (comboItemTemp != null)
                        {
                            item.Name = comboItemTemp.Text;
                        }
                        if (item.Childs != null && item.Childs.Any())
                        {
                            foreach (var child in item.Childs)
                            {
                                comboItemTemp = itemList.FirstOrDefault(x => x.Value == child.Id);
                                if (comboItemTemp != null)
                                {
                                    child.Name = comboItemTemp.Text;
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region ChartType

            if (_chartType != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "ChartType");
                if (itemList != null)
                {
                    foreach (var item in ChartType)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region ProductOrderBy

            if (_productOrderBy != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "ProductOrderBy");
                if (itemList != null)
                {
                    foreach (var item in ProductOrderBy)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region CategoryOrderBy

            if (_categoryOrderBy != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "CategoryOrderBy");
                if (itemList != null)
                {
                    foreach (var item in CategoryOrderBy)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region OrderDirection

            if (_orderDirection != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "OrderDirection");
                if (itemList != null)
                {
                    foreach (var item in OrderDirection)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region AdjustmentStatus

            if (_adjustmentStatus != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "AdjustmentStatus");
                if (itemList != null)
                {
                    foreach (var item in AdjustmentStatus)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region AdjustmentStatusAll

            if (_adjustmentStatusAll != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "AdjustmentStatus");
                if (itemList != null)
                {
                    foreach (var item in AdjustmentStatusAll)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region AdjustmentReason

            if (_adjustmentReason != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "AdjustmentReason");
                if (itemList != null)
                {
                    foreach (var item in AdjustmentReason)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }


            #endregion

            #region AdjustmentReasonAll

            if (_adjustmentReasonAll != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "AdjustmentReason");
                if (itemList != null)
                {
                    foreach (var item in AdjustmentReasonAll)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }


            #endregion

            #region AddressTypes

            if (_addressTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "AddressTypes");
                if (itemList != null)
                {
                    foreach (var item in AddressTypes)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }


            #endregion

            #region TextNumberAlignments

            if (_textNumberAlignments != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "TextNumberAlignment");
                if (itemList != null)
                {
                    foreach (var item in TextNumberAlignments)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }


            #endregion

            #region PersonalTitle

            if (_title != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "Title");
                if (itemList != null)
                {
                    foreach (var item in Title)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }


            #endregion

            #region PriorityList

            if (_priorityList != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "Priority");
                if (itemList != null)
                {
                    foreach (var item in PriorityList)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region ReminderCategories

            if (_reminderCategories != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "ReminderCategory");
                if (itemList != null)
                {
                    foreach (var item in ReminderCategories)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region ReminderRepeatList

            if (_reminderRepeatList != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "ReminderRepeat");
                if (itemList != null)
                {
                    foreach (var item in ReminderRepeatList)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }

            #endregion

            #region PaymentSchedule

            if (_paymentSchedule != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "PaymentSchedule");
                if (itemList != null)
                {
                    foreach (var item in ReminderRepeatList)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }
            #endregion

            #region CashList

            if (_cashList != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "CashList");
                if (itemList != null)
                {
                    foreach (var item in CashList)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }
            #endregion

            #region DrawerList

            if (_drawerList != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "DrawerList");
                if (itemList != null)
                {
                    foreach (var item in DrawerList)
                    {
                        comboItemTemp = itemList.FirstOrDefault(x => x.Value == item.Value);
                        if (comboItemTemp != null)
                        {
                            item.Text = comboItemTemp.Text;
                        }
                    }
                }
            }
            #endregion
        }

        #endregion

        #endregion
    }
}