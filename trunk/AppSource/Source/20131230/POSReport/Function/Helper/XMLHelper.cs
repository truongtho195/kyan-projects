using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.POSReport.Model;
using System.IO;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Reflection;

namespace CPC.POSReport.Function
{
    public class XMLHelper
    {
        #region -Properties- 
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

        #region TransferStockStatus

        private static IList<ComboItem> _transferStockStatus;
        public static IList<ComboItem> TransferStockStatus
        {
            get
            {
                if (null == _transferStockStatus)
                    _transferStockStatus = GetElements("TransferStockStatus", true);
                return _transferStockStatus;
            }
            private set
            {
                _transferStockStatus = value;
            }
        }

        #endregion

        #region StatusSalesOrders

        private static IList<ComboItem> _salesOrdersStatus;
        /// <summary>
        /// Gets the Sales Order Status
        /// </summary>
        public static IList<ComboItem> SalesOrdersStatus
        {
            get
            {
                if (_salesOrdersStatus == null)
                    _salesOrdersStatus = GetElements("SaleStatus", true);
                return _salesOrdersStatus;
            }
            private set
            {
                _salesOrdersStatus = value;
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
                    _adjustmentStatus = GetElements("AdjustmentStatus", true);
                return _adjustmentStatus;
            }
            private set
            {
                _adjustmentStatus = value;
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
                    _adjustmentReason = GetElements("AdjustmentReason", true);
                return _adjustmentReason;
            }
            private set
            {
                _adjustmentReason = value;
            }
        }

        #endregion

        #region PurchaseStatus

        private static IList<ComboItem> _purchaseStatus;
        /// <summary>
        /// Gets or sets the PurchaseStatus
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

        #region Country

        private static IList<ComboItem> _country;
        /// <summary>
        /// Gets or sets the Country
        /// </summary>
        public static IList<ComboItem> Country
        {
            get
            {
                if (null == _country)
                    _country = GetElements("country", true);
                return _country;
            }
            private set
            {
                _country = value;
            }
        }

        #endregion

        #region State

        private static IList<ComboItem> _state;
        /// <summary>
        /// Gets or sets the State
        /// </summary>
        public static IList<ComboItem> State
        {
            get
            {
                if (null == _state)
                    _state = GetElements("state", true);
                return _state;
            }
            private set
            {
                _state = value;
            }
        }

        #endregion

        #region -Payment Method-

        private static IList<ComboItem> _paymentMethods;
        /// <summary>
        /// Gets or sets the State
        /// </summary>
        public static IList<ComboItem> PaymentMethods
        {
            get
            {
                if (null == _paymentMethods)
                    _paymentMethods = GetElements("PaymentMethods", true);
                return _paymentMethods;
            }
            private set
            {
                _paymentMethods = value;
            }
        }

        #endregion

        #region Status basic

        private static IList<ComboItem> _statusBasic;
        /// <summary>
        /// Gets or sets the State
        /// </summary>
        public static IList<ComboItem> StatusBasic
        {
            get
            {
                if (null == _statusBasic)
                    _statusBasic = GetElements("StatusBasic", true);
                return _statusBasic;
            }
            private set
            {
                _statusBasic = value;
            }
        }

        #endregion
        #endregion 

        #region Fields

        private static string _comboElement = "combo";

        #endregion

        #region -Contructor-
        public XMLHelper()
        {
        }
        #endregion

        #region -methods-

        public IList<ComboItem> GetAllPaperSize()
        {
            return GetElements("PaperSizes", true);
        }
               
        public string GetName(int key, string type)
        {
            ComboItem cboItem = new ComboItem();
            switch (type)
            {
                case "TransferStockStatus":
                    cboItem = TransferStockStatus.SingleOrDefault(x => x.Value == key);
                    break;
                case "SalesOrdersStatus":
                    cboItem = SalesOrdersStatus.Single(x => x.Value == key);
                    break;
                case "AdjustmentStatus":
                    cboItem = AdjustmentStatus.Single(x => x.Value == key);
                    break;
                case "AdjustmentReason":
                    cboItem = AdjustmentReason.Single(x => x.Value == key);
                    break;
                case "PurchaseStatus":
                    cboItem = PurchaseStatus.Single(x => x.Value == key);
                    break;
                case "Country":
                    cboItem = Country.Single(x => x.Value == key);
                    break;
                case "State":
                    cboItem = State.Single(x => x.Value == key);
                    break;
                case "PaymentMethods":
                    cboItem = PaymentMethods.Single(x => x.Value == key);
                    break;
                case "StatusBasic":
                    cboItem = StatusBasic.Single(x => x.Value == key);
                    break;
            }
            if (cboItem != null)
            {
                return cboItem.Text;
            }
            return string.Empty;
        }

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
                    bool isLanguageComboItem = string.Compare(key, "Language", true) == 0;

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
                        if (isLanguageComboItem)
                        {
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
                        }

                        if ((!isAll && comboItem.Value > 0) || isAll || item.Element("value") == null)
                            comboItems.Add(comboItem);
                    }
                }
            }
            if (key == "PaperSizes")
            {
                return comboItems;
            }
            return comboItems.OrderBy(o=> o.Text).ToList();
        }

        #endregion

        #region LoadCurrentLanguagePackage

        private static Stream LoadCurrentLanguagePackage()
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

        #endregion
    }  
}
