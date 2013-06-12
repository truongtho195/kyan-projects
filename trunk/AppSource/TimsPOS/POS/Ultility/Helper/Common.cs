using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using CPC.POS.Model;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Threading;

namespace CPC.Helper
{
    internal partial class Common
    {
        #region Fields

        private static string _comboElement = "combo";
        private static string _countryElement = "country";
        private static string _stateElement = "state";
        private static string _groupofdropElement = "groupofdrop";
        private static string _statusElement = "status";

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
                    _employeeTypes = GetXmlCombo("EmployeeTypes");
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
                    _employeeCommissionTypes = GetXmlCombo("EmployeeCommissionTypes");
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
                    _payrollTypes = GetXmlCombo("PayrollTypes");
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
                    _maritalStatus = GetXmlCombo("MaritalStatus");
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
                    _gender = GetXmlCombo("Gender");
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
                    _scheduleTypes = GetXmlCombo("ScheduleTypes");
                return _scheduleTypes;
            }
            private set
            {
                _scheduleTypes = value;
            }
        }

        #endregion

        #region Statuses

        public static IList<StatusItem> Statuses = new List<StatusItem>
        {
            new StatusItem { Value = 0, Text = "Open" },
            new StatusItem { Value = 1, Text = "Processing" },
            new StatusItem { Value = 2, Text = "Complete" }
        };

        #endregion

        #region ScheduleStatuses

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

        #endregion

        #region Countries

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

        #endregion

        #region States

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

        #endregion

        #region ItemTypes

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

        #endregion

        #region MemberTypes

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
                    _purchaseStatus = GetXmlCombo("PurchaseStatus", true);
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
                    _weeksOfMonth = GetXmlCombo("WeeksOfMonth");
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
                    _daysOfWeek = GetXmlCombo("DaysOfWeek");
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
                    _months = GetXmlCombo("Months");
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
                    _workPermissionTypes = GetXmlCombo("WorkPermissionTypes");
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
                    _warrantyPeriodTypes = GetXmlCombo("WarrantyPeriodType");
                return _warrantyPeriodTypes;
            }
            private set
            {
                _warrantyPeriodTypes = value;
            }
        }

        #endregion

        #region ProductCommissionTypes

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

        #endregion

        #region WarrantyTypes

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
                    _overtimeTypes = GetXmlCombo("OvertimeTypes");
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
                    _userTypes = GetXmlCombo("UserTypes");
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
                    _payEvents = GetXmlCombo("PayEvents", true);
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
                    _customerTypes = GetXmlCombo("CustomerType");
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
                    _markdownPriceLevels = GetXmlCombo("PriceSchemas", true);
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

        #endregion

        #region StatusBasicAll

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
                if (null == _jobTitles)
                    _jobTitles = GetXmlCombo("JobTitles", true);
                return _jobTitles;
            }
            private set
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
                    _paymentCardTypes = GetXmlCombo("PaymentCardType");
                return _paymentCardTypes;
            }
            private set
            {
                _paymentCardTypes = value;
            }
        }

        #endregion

        #region GiftCardTypes

        private static IList<ComboItem> _giftCardTypes;
        /// <summary>
        /// Gets the GiftCardTypes
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

        #endregion

        #region PromotionStatuses

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
                    _promotionTypes = GetXmlCombo("PromotionTypes");
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
                    _promotionTypeAll = GetXmlCombo("PromotionTypes", true);
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
                    _takeOffOptions = GetXmlCombo("TakeOffOptions");
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
                    _priceSchemas = GetXmlCombo("PriceSchemas");
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
                    _paymentMethods = GetXmlCombo("PaymentMethods");
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
                    _bookingChannel = GetXmlCombo("BookingChannel");
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
                    _statusSalesOrders = GetXmlCombo("SaleStatus");
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
                    _languages = GetXmlLanguage();
                return _languages;
            }
            private set
            {
                _languages = value;
            }
        }

        #endregion

        #region Shifts

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
                    _stockStatus = GetXmlCombo("StockStatus");
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
                    _rewardAmountTypes = GetXmlCombo("RewardAmount", true);
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
                    _rewardExpirationTypes = GetXmlCombo("RewardExpiration", true);
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
                    _warrantyTypeAll = GetXmlCombo("WarrantyType", true);
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
                    _warrantyPeriodTypeAll = GetXmlCombo("WarrantyPeriodType", true);
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
                    _basePriceTypes = GetXmlCombo("BasePriceType");
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
                    _adjustmentTypes = GetXmlCombo("PricingAdjustmentType");
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
                    _pricingStatus = GetXmlCombo("PricingStatus");
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
                    _shipUnits = GetXmlCombo("ShipUnit");
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
                    _transferStockStatus = GetXmlCombo("TransferStockStatus");
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
                    _countStockStatus = GetXmlCombo("CountStockStatus");
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
                    _availableQuantities = GetXmlCombo("AvailableQuantity");
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
                    _guestRewardStatus = GetXmlCombo("RewardStatus");
                return _guestRewardStatus;
            }
            private set
            {
                _guestRewardStatus = value;
            }
        }
        #endregion

        #endregion

        #region Read Xml Methods

        #region GetXmlCombo

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

                var query = from p in doc.Root.Elements(_comboElement)
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

        #endregion

        #region GetXmlCountry

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

                var query = from p in doc.Root.Elements(_countryElement)
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

        #endregion

        #region GetXmlState

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

                var query = from p in doc.Root.Elements(_stateElement)
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

        #endregion

        #region GetXmlGroupOfDrop

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

                var query = from p in doc.Root.Elements(_groupofdropElement)
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

        #endregion

        #region GetXmlStatus

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

                var query = from p in doc.Root.Elements(_statusElement)
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
                        };

                        if ((!isAll && statusItem.Value > 0) || isAll)
                            statusItems.Add(statusItem);
                    }
                }
            }
            return statusItems;
        }

        #endregion

        #region GetXmlLanguage

        //To get language
        private static IList<ComboItem> GetXmlLanguage()
        {
            IList<ComboItem> languages = new List<ComboItem>();
            using (Stream stream = LoadCurrentLanguagePackage())
            {
                if (stream == null)
                {
                    return languages;
                }

                XDocument doc = XDocument.Load(stream);
                var query = from p in doc.Root.Elements(_comboElement)
                            where p.Attribute("key").Value == "Language"
                            select p;
                if (null != query)
                {
                    foreach (var item in query.Single().Elements())
                    {
                        ComboItem comboItem = new ComboItem();
                        comboItem.Value = Convert.ToInt16(item.Element("value").Value);
                        comboItem.Text = item.Element("name").Value;
                        comboItem.CultureInfo = new CultureInfo(item.Element("culture").Value);
                        comboItem.Code = item.Element("code").Value;
                        switch (comboItem.Value)
                        {
                            case 1:
                                comboItem.Image = Application.Current.FindResource("VietNamFlag") as Brush;
                                break;

                            case 2:
                                comboItem.Image = Application.Current.FindResource("EnglishFlag") as Brush;
                                break;

                            case 3:
                                comboItem.Image = Application.Current.FindResource("ChinaFlag") as Brush;
                                break;
                        }

                        languages.Add(comboItem);
                    }
                }
            }
            return languages;
        }

        #endregion

        #region GetXmlShift

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
                var query = from p in doc.Root.Elements(_comboElement)
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

        #region GetElementsOnlyValueText

        /// <summary>
        /// Gets all elements in document only value and text based on name and key of elements.
        /// </summary>
        /// <param name="xDocument">XDocument that contains all elements.</param>
        /// <param name="elementName">Name of element.</param>
        /// <param name="key">Key of element.</param>
        /// <returns></returns>
        private static IList<ComboItem> GetElementsOnlyValueText(XDocument xDocument, string elementName, string key)
        {
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

            if (null != query)
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

            // Chu y: khi doi ngon ngu 1 danh sach, dieu kien kiem tra su dung private field
            // nhu _userTypes != null, khong kiem tra public field UserTypes != null.

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

            #region JobTitles

            if (_jobTitles != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "JobTitles");
                if (itemList != null)
                {
                    foreach (var item in JobTitles)
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

            #region GiftCardTypes

            if (_giftCardTypes != null)
            {
                itemList = GetElementsOnlyValueText(xDocument, _comboElement, "GiftCardType");
                if (itemList != null)
                {
                    foreach (var item in GiftCardTypes)
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
                itemList = GetElementsOnlyValueText(xDocument, _statusElement, "StatusBasic");
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
                itemList = GetElementsOnlyValueText(xDocument, _statusElement, "StatusBasic");
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
                itemList = GetElementsOnlyValueText(xDocument, _statusElement, "StatusBasic");
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

            // Chu y: khi doi ngon ngu 1 danh sach, dieu kien kiem tra su dung private field
            // nhu _userTypes != null, khong kiem tra public field UserTypes != null.
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Thung rac
    /// </summary>
    internal partial class Common
    {
        //#region EmployeeStatuses

        //private static IList<StatusItem> _employeeStatuses;
        //public static IList<StatusItem> EmployeeStatuses
        //{
        //    get
        //    {
        //        if (null == _employeeStatuses)
        //            _employeeStatuses = GetXmlStatus("EmployeeStatuses", true);
        //        return _employeeStatuses;
        //    }
        //    private set
        //    {
        //        _employeeStatuses = value;
        //    }
        //}

        //#endregion
        //#region SalaryTypes

        //private static IList<ComboItem> _salaryTypes;
        //public static IList<ComboItem> SalaryTypes
        //{
        //    get
        //    {
        //        if (null == _salaryTypes)
        //            _salaryTypes = GetXmlCombo("SalaryTypes");
        //        return _salaryTypes;
        //    }
        //    private set
        //    {
        //        _salaryTypes = value;
        //    }
        //}

        //#endregion
        //#region AddressTypes

        //private static IList<ComboItem> _addressTypes;
        //public static IList<ComboItem> AddressTypes
        //{
        //    get
        //    {
        //        if (null == _addressTypes)
        //            _addressTypes = GetXmlCombo("AddressTypes");
        //        return _addressTypes;
        //    }
        //    private set
        //    {
        //        _addressTypes = value;
        //    }
        //}

        //#endregion

        //#region GroupOfDrops

        //private static IList<GroupOfDrop> _groupOfDrop;
        //public static IList<GroupOfDrop> GroupOfDrops
        //{
        //    get
        //    {
        //        if (null == _groupOfDrop)
        //            _groupOfDrop = GetXmlGroupOfDrop();
        //        return _groupOfDrop;
        //    }
        //    private set
        //    {
        //        _groupOfDrop = value;
        //    }
        //}

        //#endregion

        //#region TransactionTypes

        //private static IList<ComboItem> _transactionTypes;
        //public static IList<ComboItem> TransactionTypes
        //{
        //    get
        //    {
        //        if (null == _transactionTypes)
        //            _transactionTypes = GetXmlCombo("TransactionTypes");
        //        return _transactionTypes;
        //    }
        //    private set
        //    {
        //        _transactionTypes = value;
        //    }
        //}

        //#endregion

        //#region CreditStatuses

        //private static IList<StatusItem> _creditStatuses;
        //public static IList<StatusItem> CreditStatuses
        //{
        //    get
        //    {
        //        if (null == _creditStatuses)
        //            _creditStatuses = GetXmlStatus("CreditStatuses");
        //        return _creditStatuses;
        //    }
        //    private set
        //    {
        //        _creditStatuses = value;
        //    }
        //}

        //#endregion


        //#region ProductStatuses

        //private static IList<StatusItem> _productStatuses;
        //public static IList<StatusItem> ProductStatuses
        //{
        //    get
        //    {
        //        if (null == _productStatuses)
        //            _productStatuses = GetXmlStatus("ProductStatuses");
        //        return _productStatuses;
        //    }
        //    private set
        //    {
        //        _productStatuses = value;
        //    }
        //}

        //#endregion

        //#region LengthOfInsuranceTypes

        //private static IList<ComboItem> _lengthOfInsuranceTypes;
        //public static IList<ComboItem> LengthOfInsuranceTypes
        //{
        //    get
        //    {
        //        if (null == _lengthOfInsuranceTypes)
        //            _lengthOfInsuranceTypes = GetXmlCombo("LengthOfInsuranceTypes");
        //        return _lengthOfInsuranceTypes;
        //    }
        //    private set
        //    {
        //        _lengthOfInsuranceTypes = value;
        //    }
        //}

        //#endregion


        //#region GroupPermissionTypes

        //private static IList<ComboItem> _groupPermissionTypes;
        //public static IList<ComboItem> GroupPermissionTypes
        //{
        //    get
        //    {
        //        if (_groupPermissionTypes == null)
        //            _groupPermissionTypes = GetXmlCombo("GroupPermissionTypes");
        //        return _groupPermissionTypes;
        //    }
        //    private set
        //    {
        //        _groupPermissionTypes = value;
        //    }
        //}

        //#endregion

        //#region GroupPermissionStatuses

        //private static IList<StatusItem> _groupPermissionStatuses;
        //public static IList<StatusItem> GroupPermissionStatuses
        //{
        //    get
        //    {
        //        if (null == _groupPermissionStatuses)
        //            _groupPermissionStatuses = GetXmlStatus("GroupPermissionStatuses");
        //        return _groupPermissionStatuses;
        //    }
        //    private set
        //    {
        //        _groupPermissionStatuses = value;
        //    }
        //}

        //#endregion

        //#region CustomerMemberTypes

        //private static IList<ComboItem> _customerMemberTypes;
        ///// <summary>
        ///// Type of Customer 
        /////<para> 1: None Member</para>
        /////<para> 2: Platinum</para>
        /////<para> 3: Gold</para>
        /////<para> 4: Silver</para>
        /////<para> 5: Bronze</para>
        ///// </summary>
        ///// <remarks>
        ///// Create By : Kyan 28/01/2013
        ///// </remarks>
        //public static IList<ComboItem> CustomerMemberTypes
        //{
        //    get
        //    {
        //        if (_customerMemberTypes == null)
        //            _customerMemberTypes = GetXmlCombo("CustomerMemberType");
        //        return _customerMemberTypes;
        //    }
        //    private set
        //    {
        //        _customerMemberTypes = value;
        //    }
        //}

        //#endregion
    }

}