using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using CPC.POS.Model;

namespace CPC.Helper
{
    internal partial class Common
    {
        #region Constant & members

        private static string _languageFile = "en-US.xml";
        private static string _languageFolder = "XML";

        #endregion

        #region Properties

        // EmployeeTypes
        private static IList<ComboItem> _employeeTypes;
        public static IList<ComboItem> EmployeeTypes
        {
            get
            {
                if (null == _employeeTypes)
                    _employeeTypes = GetXmlCombo("EmployeeTypes");
                return _employeeTypes;
            }
            private set
            {
                _employeeTypes = value;
            }
        }

        // EmployeeStatuses
        private static IList<StatusItem> _employeeStatuses;
        public static IList<StatusItem> EmployeeStatuses
        {
            get
            {
                if (null == _employeeStatuses)
                    _employeeStatuses = GetXmlStatus("EmployeeStatuses", true);
                return _employeeStatuses;
            }
            private set
            {
                _employeeStatuses = value;
            }
        }

        // CommissionTypes
        private static IList<ComboItem> _employeeCommissionTypes;
        public static IList<ComboItem> EmployeeCommissionTypes
        {
            get
            {
                if (null == _employeeCommissionTypes)
                    _employeeCommissionTypes = GetXmlCombo("EmployeeCommissionTypes");
                return _employeeCommissionTypes;
            }
            private set
            {
                _employeeCommissionTypes = value;
            }
        }

        // AnalysisReports
        //private static IList<ReportItem> _analysisReports;
        //public static IList<ReportItem> AnalysisReports
        //{
        //    get
        //    {
        //        if (null == _analysisReports)
        //            _analysisReports = GetXmlReport("AnalysisReports");
        //        return _analysisReports;
        //    }
        //}

        // SalaryTypes
        private static IList<ComboItem> _salaryTypes;
        public static IList<ComboItem> SalaryTypes
        {
            get
            {
                if (null == _salaryTypes)
                    _salaryTypes = GetXmlCombo("SalaryTypes");
                return _salaryTypes;
            }
            private set
            {
                _salaryTypes = value;
            }
        }

        // PayrollTypes
        private static IList<ComboItem> _payrollTypes;
        public static IList<ComboItem> PayrollTypes
        {
            get
            {
                if (null == _payrollTypes)
                    _payrollTypes = GetXmlCombo("PayrollTypes");
                return _payrollTypes;
            }
            private set
            {
                _payrollTypes = value;
            }
        }

        // AddressTypes
        private static IList<ComboItem> _addressTypes;
        public static IList<ComboItem> AddressTypes
        {
            get
            {
                if (null == _addressTypes)
                    _addressTypes = GetXmlCombo("AddressTypes");
                return _addressTypes;
            }
            private set
            {
                _addressTypes = value;
            }
        }

        // MaritalStatus
        private static IList<ComboItem> _maritalStatus;
        public static IList<ComboItem> MaritalStatus
        {
            get
            {
                if (null == _maritalStatus)
                    _maritalStatus = GetXmlCombo("MaritalStatus");
                return _maritalStatus;
            }
            private set
            {
                _maritalStatus = value;
            }
        }

        // Gender
        private static IList<ComboItem> _gender;
        public static IList<ComboItem> Gender
        {
            get
            {
                if (null == _gender)
                    _gender = GetXmlCombo("Gender");
                return _gender;
            }
            private set
            {
                _gender = value;
            }
        }

        // Gender
        private static IList<ComboItem> _scheduleTypes;
        public static IList<ComboItem> ScheduleTypes
        {
            get
            {
                if (null == _scheduleTypes)
                    _scheduleTypes = GetXmlCombo("ScheduleTypes");
                return _scheduleTypes;
            }
            private set
            {
                _scheduleTypes = value;
            }
        }

        // Statuses
        public static IList<StatusItem> Statuses = new List<StatusItem>
        {
            new StatusItem { Value = 0, Text = "Open" },
            new StatusItem { Value = 1, Text = "Processing" },
            new StatusItem { Value = 2, Text = "Complete" }
        };

        // ScheduleStatuses
        private static IList<StatusItem> _scheduleStatuses;
        public static IList<StatusItem> ScheduleStatuses
        {
            get
            {
                if (null == _scheduleStatuses)
                    _scheduleStatuses = GetXmlStatus("ScheduleStatuses");
                return _scheduleStatuses;
            }
            private set
            {
                _scheduleStatuses = value;
            }
        }

        // Countries
        private static IList<CountryItem> _countries;
        public static IList<CountryItem> Countries
        {
            get
            {
                if (null == _countries)
                    _countries = GetXmlCountry();
                return _countries;
            }
            private set
            {
                _countries = value;
            }
        }

        // States
        private static IList<StateItem> _states;
        public static IList<StateItem> States
        {
            get
            {
                if (null == _states)
                    _states = GetXmlState();
                return _states;
            }
            private set
            {
                _states = value;
            }
        }

        // States
        private static IList<GroupOfDrop> _groupOfDrop;
        public static IList<GroupOfDrop> GroupOfDrops
        {
            get
            {
                if (null == _groupOfDrop)
                    _groupOfDrop = GetXmlGroupOfDrop();
                return _groupOfDrop;
            }
            private set
            {
                _groupOfDrop = value;
            }
        }

        // Item Types
        private static IList<ComboItem> _itemTypes;
        public static IList<ComboItem> ItemTypes
        {
            get
            {
                if (null == _itemTypes)
                    _itemTypes = GetXmlCombo("ItemTypes", true);
                return _itemTypes;
            }
            private set
            {
                _itemTypes = value;
            }
        }

        // Transaction Types
        private static IList<ComboItem> _transactionTypes;
        public static IList<ComboItem> TransactionTypes
        {
            get
            {
                if (null == _transactionTypes)
                    _transactionTypes = GetXmlCombo("TransactionTypes");
                return _transactionTypes;
            }
            private set
            {
                _transactionTypes = value;
            }
        }

        // Member Types
        private static IList<ComboItem> _memberTypes;
        public static IList<ComboItem> MemberTypes
        {
            get
            {
                if (null == _memberTypes)
                    _memberTypes = GetXmlCombo("MemberType");
                return _memberTypes;
            }
            private set
            {
                _memberTypes = value;
            }
        }

        private static IList<ComboItem> _purchaseStatus;
        /// <summary>
        /// Gets Purchase Statuses.
        /// </summary>
        public static IList<ComboItem> PurchaseStatus
        {
            get
            {
                if (null == _purchaseStatus)
                    _purchaseStatus = GetXmlCombo("PurchaseStatus", true);
                return _purchaseStatus;
            }
            private set
            {
                _purchaseStatus = value;
            }
        }

        // Credit Statuses
        private static IList<StatusItem> _creditStatuses;
        public static IList<StatusItem> CreditStatuses
        {
            get
            {
                if (null == _creditStatuses)
                    _creditStatuses = GetXmlStatus("CreditStatuses");
                return _creditStatuses;
            }
            private set
            {
                _creditStatuses = value;
            }
        }

        // Weeks of Month
        private static IList<ComboItem> _weeksOfMonth;
        public static IList<ComboItem> WeeksOfMonth
        {
            get
            {
                if (null == _weeksOfMonth)
                    _weeksOfMonth = GetXmlCombo("WeeksOfMonth");
                return _weeksOfMonth;
            }
            private set
            {
                _weeksOfMonth = value;
            }
        }

        // Days of Week
        private static IList<ComboItem> _daysOfWeek;
        public static IList<ComboItem> DaysOfWeek
        {
            get
            {
                if (null == _daysOfWeek)
                    _daysOfWeek = GetXmlCombo("DaysOfWeek");
                return _daysOfWeek;
            }
            private set
            {
                _daysOfWeek = value;
            }
        }

        // Months
        private static IList<ComboItem> _months;
        public static IList<ComboItem> Months
        {
            get
            {
                if (null == _months)
                    _months = GetXmlCombo("Months");
                return _months;
            }
            private set
            {
                _months = value;
            }
        }

        private static IList<StatusItem> _productStatuses;
        public static IList<StatusItem> ProductStatuses
        {
            get
            {
                if (null == _productStatuses)
                    _productStatuses = GetXmlStatus("ProductStatuses");
                return _productStatuses;
            }
            private set
            {
                _productStatuses = value;
            }
        }

        private static IList<ComboItem> _workPermissionTypes;
        public static IList<ComboItem> WorkPermissionType
        {
            get
            {
                if (_workPermissionTypes == null)
                    _workPermissionTypes = GetXmlCombo("WorkPermissionTypes");
                return _workPermissionTypes;
            }
            private set
            {
                _workPermissionTypes = value;
            }
        }

        // WarrantyPeriod Types.
        private static IList<ComboItem> _warrantyPeriodTypes;
        public static IList<ComboItem> WarrantyPeriodTypes
        {
            get
            {
                if (null == _warrantyPeriodTypes)
                    _warrantyPeriodTypes = GetXmlCombo("WarrantyPeriodType");
                return _warrantyPeriodTypes;
            }
            private set
            {
                _warrantyPeriodTypes = value;
            }
        }

        // LengthOfInsurance Types.
        private static IList<ComboItem> _lengthOfInsuranceTypes;
        public static IList<ComboItem> LengthOfInsuranceTypes
        {
            get
            {
                if (null == _lengthOfInsuranceTypes)
                    _lengthOfInsuranceTypes = GetXmlCombo("LengthOfInsuranceTypes");
                return _lengthOfInsuranceTypes;
            }
            private set
            {
                _lengthOfInsuranceTypes = value;
            }
        }

        // Commission Types.
        private static IList<ComboItem> _productCommissionTypes;
        public static IList<ComboItem> ProductCommissionTypes
        {
            get
            {
                if (null == _productCommissionTypes)
                    _productCommissionTypes = GetXmlCombo("ProductCommissionTypes");
                return _productCommissionTypes;
            }
            private set
            {
                _productCommissionTypes = value;
            }
        }

        // Warranty Types.
        private static IList<ComboItem> _warrantyTypes;
        public static IList<ComboItem> WarrantyTypes
        {
            get
            {
                if (null == _warrantyTypes)
                    _warrantyTypes = GetXmlCombo("WarrantyType");
                return _warrantyTypes;
            }
            private set
            {
                _warrantyTypes = value;
            }
        }

        //Group Permission Types
        private static IList<ComboItem> _groupPermissionTypes;
        public static IList<ComboItem> GroupPermissionTypes
        {
            get
            {
                if (_groupPermissionTypes == null)
                    _groupPermissionTypes = GetXmlCombo("GroupPermissionTypes");
                return _groupPermissionTypes;
            }
            private set
            {
                _groupPermissionTypes = value;
            }
        }

        // Group Permission Statuses
        private static IList<StatusItem> _groupPermissionStatuses;
        public static IList<StatusItem> GroupPermissionStatuses
        {
            get
            {
                if (null == _groupPermissionStatuses)
                    _groupPermissionStatuses = GetXmlStatus("GroupPermissionStatuses");
                return _groupPermissionStatuses;
            }
            private set
            {
                _groupPermissionStatuses = value;
            }
        }


        private static IList<ComboItem> _overtimeTypes;
        /// <summary>
        /// 
        /// </summary>
        public static IList<ComboItem> OvertimeTypes
        {
            get
            {
                if (_overtimeTypes == null)
                    _overtimeTypes = GetXmlCombo("OvertimeTypes");
                return _overtimeTypes;
            }
            private set
            {
                _overtimeTypes = value;
            }
        }
        //UserTypes
        private static IList<ComboItem> _userTypes;
        /// <summary>
        /// 
        /// </summary>
        public static IList<ComboItem> UserTypes
        {
            get
            {
                if (_userTypes == null)
                    _userTypes = GetXmlCombo("UserTypes");
                return _userTypes;
            }
            private set
            {
                _userTypes = value;
            }
        }

        //PayEvents
        private static IList<ComboItem> _payEvents;
        /// <summary>
        /// 
        /// </summary>
        public static IList<ComboItem> PayEvents
        {
            get
            {
                if (_payEvents == null)
                    _payEvents = GetXmlCombo("PayEvents", true);
                return _payEvents;
            }
            private set
            {
                _payEvents = value;
            }
        }


        //Customer Type
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
                    _customerTypes = GetXmlCombo("CustomerType");
                return _customerTypes;
            }
            private set
            {
                _customerTypes = value;
            }
        }

        //CustomerMemberTypes
        private static IList<ComboItem> _customerMemberTypes;
        /// <summary>
        /// Type of Customer 
        ///<para> 1: None Member</para>
        ///<para> 2: Platinum</para>
        ///<para> 3: Gold</para>
        ///<para> 4: Silver</para>
        ///<para> 5: Bronze</para>
        /// </summary>
        /// <remarks>
        /// Create By : Kyan 28/01/2013
        /// </remarks>
        public static IList<ComboItem> CustomerMemberTypes
        {
            get
            {
                if (_customerMemberTypes == null)
                    _customerMemberTypes = GetXmlCombo("CustomerMemberType");
                return _customerMemberTypes;
            }
            private set
            {
                _customerMemberTypes = value;
            }
        }


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
                    _markdownPriceLevels = GetXmlCombo("PriceSchemas", true);
                return _markdownPriceLevels;
            }
            private set
            {
                _markdownPriceLevels = value;
            }
        }


        // Basic Status
        private static IList<StatusItem> _statusBasic;
        /// <summary>
        /// Status for Combobox with foreground & background Color
        /// <para>1: Deactive | BackColor : Red | ForeColor : White</para>
        /// <para>2: Active | BackColor: Green | ForeColor : Black </para>
        /// </summary>
        public static IList<StatusItem> StatusBasic
        {
            get
            {
                if (null == _statusBasic)
                    _statusBasic = GetXmlStatus("StatusBasic");
                return _statusBasic;
            }
            private set
            {
                _statusBasic = value;
            }
        }

        private static IList<StatusItem> _statusBasicAll;
        /// <summary>
        /// Status for Combobox with foreground & background Color
        /// <para>1: Deactive | BackColor : Red | ForeColor : White</para>
        /// <para>2: Active | BackColor: Green | ForeColor : Black </para>
        /// </summary>
        public static IList<StatusItem> StatusBasicAll
        {
            get
            {
                if (null == _statusBasicAll)
                    _statusBasicAll = GetXmlStatus("StatusBasic", true);
                return _statusBasicAll;
            }
            private set
            {
                _statusBasicAll = value;
            }
        }

        private static IList<ComboItem> _jobTitles;
        /// <summary>
        /// Gets the JobTitles
        /// </summary>
        public static IList<ComboItem> JobTitles
        {
            get
            {
                if (null == _jobTitles)
                    _jobTitles = GetXmlCombo("JobTitles", true);
                return _jobTitles;
            }
            private set
            {
                _jobTitles = value;
            }
        }

        private static IList<ComboItem> _paymentCardTypes;
        /// <summary>
        /// Gets the PaymentCardTypes
        /// </summary>
        public static IList<ComboItem> PaymentCardTypes
        {
            get
            {
                if (null == _paymentCardTypes)
                    _paymentCardTypes = GetXmlCombo("PaymentCardType");
                return _paymentCardTypes;
            }
            private set
            {
                _paymentCardTypes = value;
            }
        }

        private static IList<ComboItem> _giftCardTypes;
        /// <summary>
        /// Gets the PaymentCardTypes
        /// </summary>
        public static IList<ComboItem> GiftCardTypes
        {
            get
            {
                if (null == _giftCardTypes)
                    _giftCardTypes = GetXmlCombo("GiftCardType");
                return _giftCardTypes;
            }
            private set
            {
                _giftCardTypes = value;
            }
        }

        private static IList<StatusItem> _promotionStatuses;
        /// <summary>
        /// Gets the PromotionStatuses
        /// </summary>
        public static IList<StatusItem> PromotionStatuses
        {
            get
            {
                if (null == _promotionStatuses)
                    _promotionStatuses = GetXmlStatus("StatusBasic");
                return _promotionStatuses;
            }
            private set
            {
                _promotionStatuses = value;
            }
        }

        private static IList<ComboItem> _promotionTypes;
        /// <summary>
        /// Gets the PromotionTypes
        /// </summary>
        public static IList<ComboItem> PromotionTypes
        {
            get
            {
                if (null == _promotionTypes)
                    _promotionTypes = GetXmlCombo("PromotionTypes");
                return _promotionTypes;
            }
            private set
            {
                _promotionTypes = value;
            }
        }

        private static IList<ComboItem> _promotionTypeAll;
        /// <summary>
        /// Gets the PromotionTypes
        /// </summary>
        public static IList<ComboItem> PromotionTypeAll
        {
            get
            {
                if (null == _promotionTypeAll)
                    _promotionTypeAll = GetXmlCombo("PromotionTypes", true);
                return _promotionTypeAll;
            }
            private set
            {
                _promotionTypeAll = value;
            }
        }

        private static IList<ComboItem> _takeOffOptions;
        /// <summary>
        /// Gets the TakeOffOptions
        /// </summary>
        public static IList<ComboItem> TakeOffOptions
        {
            get
            {
                if (null == _takeOffOptions)
                    _takeOffOptions = GetXmlCombo("TakeOffOptions");
                return _takeOffOptions;
            }
            private set
            {
                _takeOffOptions = value;
            }
        }

        private static IList<ComboItem> _priceSchemas;
        /// <summary>
        /// Gets the PriceSchemas
        /// </summary>
        public static IList<ComboItem> PriceSchemas
        {
            get
            {
                if (_priceSchemas == null)
                    _priceSchemas = GetXmlCombo("PriceSchemas");
                return _priceSchemas;
            }
            private set
            {
                _priceSchemas = value;
            }
        }

        private static IList<ComboItem> _paymentMethods;
        /// <summary>
        /// Gets the PaymentMethods
        /// </summary>
        public static IList<ComboItem> PaymentMethods
        {
            get
            {
                if (_paymentMethods == null)
                    _paymentMethods = GetXmlCombo("PaymentMethods");
                return _paymentMethods;
            }
            private set
            {
                _paymentMethods = value;
            }
        }

        private static IList<ComboItem> _bookingChannel;
        /// <summary>
        /// Gets the PaymentMethods
        /// </summary>
        public static IList<ComboItem> BookingChannel
        {
            get
            {
                if (_bookingChannel == null)
                    _bookingChannel = GetXmlCombo("BookingChannel");
                return _bookingChannel;
            }
            private set
            {
                _bookingChannel = value;
            }
        }

        private static IList<ComboItem> _statusSalesOrders;
        /// <summary>
        /// Gets the Sales Order Status
        /// </summary>
        public static IList<ComboItem> StatusSalesOrders
        {
            get
            {
                if (_statusSalesOrders == null)
                    _statusSalesOrders = GetXmlCombo("SaleStatus");
                return _statusSalesOrders;
            }
            private set
            {
                _statusSalesOrders = value;
            }
        }


        // Languages
        private static IList<LanguageItem> _languages;
        public static IList<LanguageItem> Languages
        {
            get
            {
                if (null == _languages)
                    _languages = GetXmlLanguage();
                return _languages;
            }
            private set
            {
                _languages = value;
            }
        }

        // Languages
        private static IList<LanguageItem> _shifts;
        public static IList<LanguageItem> Shifts
        {
            get
            {
                if (null == _shifts)
                    _shifts = GetXmlShift();
                return _shifts;
            }
            private set
            {
                _shifts = value;
            }
        }

        private static IList<ComboItem> _stockStatus;
        /// <summary>
        /// Gets or sets the StockStatus
        /// </summary>
        public static IList<ComboItem> StockStatus
        {
            get
            {
                if (_stockStatus == null)
                    _stockStatus = GetXmlCombo("StockStatus");
                return _stockStatus;
            }
            private set
            {
                _stockStatus = value;
            }
        }

        // Reward Amount
        private static IList<ComboItem> _rewardAmountTypes;
        public static IList<ComboItem> RewardAmountTypes
        {
            get
            {
                if (null == _rewardAmountTypes)
                    _rewardAmountTypes = GetXmlCombo("RewardAmount", true);
                return _rewardAmountTypes;
            }
            private set
            {
                _rewardAmountTypes = value;
            }
        }

        // Reward Expiration
        private static IList<ComboItem> _rewardExpirationTypes;
        public static IList<ComboItem> RewardExpirationTypes
        {
            get
            {
                if (null == _rewardExpirationTypes)
                    _rewardExpirationTypes = GetXmlCombo("RewardExpiration", true);
                return _rewardExpirationTypes;
            }
            private set
            {
                _rewardExpirationTypes = value;
            }
        }

        // Warranty Type All
        private static IList<ComboItem> _warrantyTypeAll;
        public static IList<ComboItem> WarrantyTypeAll
        {
            get
            {
                if (null == _warrantyTypeAll)
                    _warrantyTypeAll = GetXmlCombo("WarrantyType", true);
                return _warrantyTypeAll;
            }
            private set
            {
                _warrantyTypeAll = value;
            }
        }

        // WarrantyPeriod Type All
        private static IList<ComboItem> _warrantyPeriodTypeAll;
        public static IList<ComboItem> WarrantyPeriodTypeAll
        {
            get
            {
                if (null == _warrantyPeriodTypeAll)
                    _warrantyPeriodTypeAll = GetXmlCombo("WarrantyPeriodType", true);
                return _warrantyPeriodTypeAll;
            }
            private set
            {
                _warrantyPeriodTypeAll = value;
            }
        }

        // BasePrice Types.
        private static IList<ComboItem> _basePriceTypes;
        public static IList<ComboItem> BasePriceTypes
        {
            get
            {
                if (null == _basePriceTypes)
                    _basePriceTypes = GetXmlCombo("BasePriceType");
                return _basePriceTypes;
            }
            private set
            {
                _basePriceTypes = value;
            }
        }

        // Adjustment Types.
        private static IList<ComboItem> _adjustmentTypes;
        public static IList<ComboItem> AdjustmentTypes
        {
            get
            {
                if (null == _adjustmentTypes)
                    _adjustmentTypes = GetXmlCombo("PricingAdjustmentType");
                return _adjustmentTypes;
            }
            private set
            {
                _adjustmentTypes = value;
            }
        }
        // Adjustment Types.
        private static IList<ComboItem> _pricingStatus;
        public static IList<ComboItem> PricingStatus
        {
            get
            {
                if (null == _pricingStatus)
                    _pricingStatus = GetXmlCombo("PricingStatus");
                return _pricingStatus;
            }
            private set
            {
                _pricingStatus = value;
            }
        }

        // Ship Unit Types.
        private static IList<ComboItem> _shipUnits;
        public static IList<ComboItem> ShipUnits
        {
            get
            {
                if (null == _shipUnits)
                    _shipUnits = GetXmlCombo("ShipUnit");
                return _shipUnits;
            }
            private set
            {
                _shipUnits = value;
            }
        }

        // TransferStock Status.
        private static IList<ComboItem> _transferStockStatus;
        public static IList<ComboItem> TransferStockStatus
        {
            get
            {
                if (null == _transferStockStatus)
                    _transferStockStatus = GetXmlCombo("TransferStockStatus");
                return _transferStockStatus;
            }
            private set
            {
                _transferStockStatus = value;
            }
        }

        // CountStock Status
        private static IList<ComboItem> _countStockStatus;
        public static IList<ComboItem> CountStockStatus
        {
            get
            {
                if (null == _countStockStatus)
                    _countStockStatus = GetXmlCombo("CountStockStatus");
                return _countStockStatus;
            }
            private set
            {
                _countStockStatus = value;
            }
        }

        // AvailableQuantities
        private static IList<ComboItem> _availableQuantities;
        public static IList<ComboItem> AvailableQuantities
        {
            get
            {
                if (null == _availableQuantities)
                    _availableQuantities = GetXmlCombo("AvailableQuantity");
                return _availableQuantities;
            }
            private set
            {
                _availableQuantities = value;
            }
        }

        #endregion

        #region Xml Methods

        private static IList<ComboItem> GetXmlCombo(string key, bool isAll = false)
        {
            IList<ComboItem> comboItems = new List<ComboItem>();
            using (Stream stream = LoadCurrentLanguagePackage())
            {
                if (stream == null)
                {
                    return comboItems;
                }

                XDocument doc = XDocument.Load(stream);

                var query = from p in doc.Root.Elements("combo")
                            where p.Attribute("key").Value == key
                            select p;
                if (null != query)
                {
                    foreach (var item in query.Single().Elements())
                    {
                        ComboItem comboItem = new ComboItem
                        {
                            Value = Convert.ToInt16(item.Element("value").Value),
                            Text = item.Element("name").Value,
                            IntValue = item.Element("default") != null ? Convert.ToInt32(item.Element("default").Value) : 0,
                            ObjValue = item.Element("value").Value,
                            Flag = item.Element("flag") != null ? bool.Parse(item.Element("flag").Value) : false,
                            Group = item.Element("group") != null ? item.Element("group").Value : string.Empty,
                            Symbol = item.Element("symbol") != null ? item.Element("symbol").Value : string.Empty,
                            ParentId = item.Element("ParentId") != null ? Convert.ToInt32(item.Element("ParentId").Value) : 0,
                            Detail = item.Element("detail") != null ? item.Element("detail").Value : "0",
                            Islocked = item.Element("islocked") != null ? bool.Parse(item.Element("islocked").Value) : false
                        };

                        if ((!isAll && comboItem.Value > 0) || isAll)
                            comboItems.Add(comboItem);
                    }
                }
            }
            return comboItems;
        }

        private static IList<CountryItem> GetXmlCountry()
        {
            IList<CountryItem> countryItems = new List<CountryItem>();
            using (Stream stream = LoadCurrentLanguagePackage())
            {
                if (stream == null)
                {
                    return countryItems;
                }

                XDocument doc = XDocument.Load(stream);

                var query = from p in doc.Root.Elements("country")
                            select p;
                if (null != query)
                {
                    foreach (var item in query.Single().Elements())
                    {
                        countryItems.Add(new CountryItem
                        {
                            Value = Convert.ToInt16(item.Element("value").Value),
                            ObjValue = item.Element("value").Value,
                            Text = item.Element("name").Value,
                            Symbol = (null == item.Element("symbol") ? String.Empty : item.Element("symbol").Value),
                            HasState = (null == item.Element("hasstate") ? false : bool.Parse(item.Element("hasstate").Value))
                        });
                    }
                }
            }
            return countryItems;
        }

        private static IList<StateItem> GetXmlState()
        {
            IList<StateItem> stateItems = new List<StateItem>();
            using (Stream stream = LoadCurrentLanguagePackage())
            {
                if (stream == null)
                {
                    return stateItems;
                }

                XDocument doc = XDocument.Load(stream);

                var query = from p in doc.Root.Elements("state")
                            select p;
                if (null != query)
                {
                    foreach (var item in query.Single().Elements())
                    {
                        stateItems.Add(new StateItem
                        {
                            Value = Convert.ToInt16(item.Element("value").Value),
                            ObjValue = item.Element("value").Value,
                            Text = item.Element("name").Value,
                            Symbol = (null == item.Element("symbol") ? String.Empty : item.Element("symbol").Value),
                        });
                    }
                }
            }
            return stateItems;
        }

        //private static IList<ReportItem> GetXmlReport(string key)
        //{
        //    IList<ReportItem> reportItems = new List<ReportItem>();
        //    using (Stream stream = LoadCurrentLanguagePackage())
        //    {
        //        XDocument doc = XDocument.Load(stream);

        //        var query = from p in doc.Root.Elements("report")
        //                    where p.Attribute("key").Value == key
        //                    select p;
        //        if (null != query)
        //        {
        //            foreach (var item in query.Single().Elements())
        //            {
        //                reportItems.Add(new ReportItem
        //                {
        //                    Value = Convert.ToInt16(item.Element("value").Value),
        //                    Text = item.Element("name").Value,
        //                    Group = item.Element("group" + _language).Value
        //                });
        //            }
        //        }
        //    }
        //    return reportItems;
        //}

        private static IList<GroupOfDrop> GetXmlGroupOfDrop()
        {
            IList<GroupOfDrop> groupOfDrop = new List<GroupOfDrop>();
            using (Stream stream = LoadCurrentLanguagePackage())
            {
                if (stream == null)
                {
                    return groupOfDrop;
                }

                XDocument doc = XDocument.Load(stream);

                var query = from p in doc.Root.Elements("groupofdrop")
                            select p;
                if (null != query)
                {
                    foreach (var item in query.Single().Elements())
                    {
                        groupOfDrop.Add(new GroupOfDrop
                        {
                            Value = Convert.ToInt16(item.Element("value").Value),
                            Text = item.Element("name").Value,
                            Tab = Convert.ToInt32(item.Element("tab").Value)
                        });
                    }
                }
            }
            return groupOfDrop;
        }

        private static IList<StatusItem> GetXmlStatus(string key, bool isAll = false)
        {
            IList<StatusItem> statusItems = new List<StatusItem>();
            using (Stream stream = LoadCurrentLanguagePackage())
            {
                if (stream == null)
                {
                    return statusItems;
                }

                XDocument doc = XDocument.Load(stream);

                var query = from p in doc.Root.Elements("status")
                            where p.Attribute("key").Value == key
                            select p;
                if (null != query)
                {
                    foreach (var item in query.Single().Elements())
                    {
                        var statusItem = new StatusItem
                        {
                            Value = Convert.ToInt16(item.Element("value").Value),
                            Text = item.Element("name").Value,
                            BackColor = item.Element("backcolor").Value,
                            ForeColor = item.Element("forecolor").Value,
                        };

                        if ((!isAll && statusItem.Value > 0) || isAll)
                            statusItems.Add(statusItem);
                    }
                }
            }
            return statusItems;
        }

        //To get language
        private static IList<LanguageItem> GetXmlLanguage()
        {
            IList<LanguageItem> languages = new List<LanguageItem>();
            using (Stream stream = LoadCurrentLanguagePackage())
            {
                if (stream == null)
                {
                    return languages;
                }

                XDocument doc = XDocument.Load(stream);
                var query = from p in doc.Root.Elements("combo")
                            where p.Attribute("key").Value == "Language"
                            select p;
                if (null != query)
                {
                    foreach (var item in query.Single().Elements())
                    {
                        languages.Add(new LanguageItem
                        {
                            Value = Convert.ToInt16(item.Element("value").Value),
                            Text = item.Element("name").Value,
                            Culture = item.Element("culture").Value,
                            Code = item.Element("code").Value
                        });
                    }
                }
            }
            return languages;
        }

        //To get shift
        private static IList<LanguageItem> GetXmlShift()
        {
            IList<LanguageItem> shift = new List<LanguageItem>();
            using (Stream stream = LoadCurrentLanguagePackage())
            {
                if (stream == null)
                {
                    return shift;
                }

                XDocument doc = XDocument.Load(stream);
                var query = from p in doc.Root.Elements("combo")
                            where p.Attribute("key").Value == "Shift"
                            select p;
                if (null != query)
                {
                    foreach (var item in query.Single().Elements())
                    {
                        shift.Add(new LanguageItem
                        {
                            Value = Convert.ToInt16(item.Element("value").Value),
                            Text = item.Element("name").Value,
                            Code = item.Element("code").Value
                        });
                    }
                }
            }
            return shift;
        }


        #endregion

        #region Methods

        #region LoadCurrentLanguagePackage

        public static Stream LoadCurrentLanguagePackage()
        {
            DirectoryInfo directoryExecuting = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            DirectoryInfo dataFolder = directoryExecuting.GetDirectories(_languageFolder).FirstOrDefault();
            if (dataFolder != null)
            {
                FileInfo languagePackage = dataFolder.GetFiles(_languageFile).FirstOrDefault();
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
                    propInfo.SetValue(null, null, null);
                }
            }
        }

        #endregion

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
    }
}