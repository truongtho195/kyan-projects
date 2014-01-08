using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Management;
using System.Net.NetworkInformation;

class ProductKeyGenerator
{
    #region Ctor
    public ProductKeyGenerator()
    {
    }
    #endregion

    #region Property
    #endregion

    #region Generate & Validate
    /// <summary>
    /// General Key
    /// </summary>
    /// <param name="input"></param>
    /// <param name="isFormated"></param>
    /// <returns></returns>
    public static string Generate(string input, bool isFormated = false)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;


        MD5 md5 = MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);
        // step 2, convert byte array to hex string
        StringBuilder sb = new StringBuilder();
        int k = 0;

        for (int i = 0; i < hash.Length; i++)
        {
            k++;
            sb.Append(hash[i].ToString("X2"));
            if (k == 5 && isFormated)
            {
                k = 0;
                sb.Append("-");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Validate Product Key
    /// </summary>
    /// <param name="productModel"></param>
    /// <param name="productKey"></param>
    /// <returns></returns>
    public bool IsValid(ProductKeyModel productModel, string productKey)
    {
        if (productModel.ProductKey.ToLower().Equals(productKey.ToLower()))
            return true;
        return false;
    }


    /// <summary>
    /// Descryt Product 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string DescrytProductKey(string input)
    {
        string passKey = ToNumbericValue(GetHddSerial());
        return EncryptDecryptString.Decrypt(input, passKey);
    }

    /// <summary>
    /// Descrypt With PasKey
    /// </summary>
    /// <param name="input"></param>
    /// <param name="passKey"></param>
    /// <returns></returns>
    public static string DescrytProductKey(string input, string passKey)
    {
        return EncryptDecryptString.Decrypt(input, passKey);
    }
    #endregion

    #region Methods
   
 
    #endregion

    #region Helper
    public string ClearFormat(string input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            if (input.IndexOf('-') > 0)
                return input.Replace("-", "");
            else
                return input;
        }
        return string.Empty;

    }

    /// <summary>
    /// Any Input string to numberic string
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns></returns>
    public static string ToNumbericValue(string inputString)
    {
        if (string.IsNullOrWhiteSpace(inputString))
            return string.Empty;

        char[] charArr = inputString.ToCharArray();
        string ouput = string.Empty;
        foreach (var item in charArr)
        {
            int i = (int)(item - '0');
            ouput += i.ToString();
        }
        return ouput;
    }

    /// <summary>
    /// string to hex
    /// </summary>
    /// <param name="inputString"></param>
    public static string ToHex(string inputString)
    {
        var as_hex = inputString.Select(x => ((int)x).ToString("X02"));
        return string.Join("", as_hex);
    }

    /// <summary>
    /// String to ASII
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToASCII(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        byte[] asciiBytes = Encoding.ASCII.GetBytes(input);

        string result = string.Join("", asciiBytes);

        return result;
    }

    /// <summary>
    /// Get CPU ID
    /// </summary>
    /// <returns></returns>
    public static string GetCpuId()
    {
        string cpuid = null;
        try
        {
            //Required :Import System.Management
            ManagementObjectSearcher mo = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (var item in mo.Get())
            {
                cpuid = item["ProcessorId"].ToString();
            }
            return cpuid;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get Mac Address
    /// </summary>
    /// <returns></returns>
    public static string GetMacAddress()
    {
        string macAddresses = "";

        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            macAddresses = nic.GetPhysicalAddress().ToString();
            break;
        }
        return macAddresses;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetHddSerial()
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

        foreach (ManagementObject wmi_HD in searcher.Get())
        {
            // get the hardware serial no.
            if (wmi_HD["SerialNumber"] != null)
                return wmi_HD["SerialNumber"].ToString().Trim();
        }
        return string.Empty;
    }


    #endregion
}

class LicenseModel
{
    #region Properties
    public string ApplicationId { get; set; }
    public int TotalStore { get; set; }

    #region LisenceKey

    /// <summary>
    /// Gets the LisenceKey.
    /// </summary>
    public string LisenceKey
    {
        get
        {
            return this.GenerateKey();
        }

    }
    #endregion

    #endregion

    #region Constructor
    public LicenseModel(string applicationId, int totalStore)
    {
        this.ApplicationId = applicationId;
        this.TotalStore = totalStore;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Method General To MD5 key
    /// </summary>
    /// <returns></returns>
    private string GenerateKey()
    {
        string result = string.Empty;
        if (!string.IsNullOrWhiteSpace(this.ToString()))
            result = ProductKeyGenerator.Generate(this.ToString());
        return result;
    }
    #endregion

    #region Override
    public override string ToString()
    {
        return string.Format("{0}{1}", ApplicationId, TotalStore);
    }

    #endregion
}

class ProductKeyModel
{
    private LicenseModel LicenseModel { get; set; }
    private int StoreCode { get; set; }
    private string PosID { get; set; }
    private int ExpiredDate { get; set; }
    private int ProjectId { get; set; }

    public string ProductKey
    {
        get
        {
            return GenerateKey();
        }
    }

    #region Ctor
    /// <summary>
    /// Initial Product key
    /// </summary>
    /// <param name="licenseModel">fix information not to descryt with genereate to md5</param>
    /// <param name="storeCode">identify is main store or substore</param>
    /// <param name="posId"></param>
    /// <param name="twoWay">Can descryt Product key to get StoreCode,PosId? </param>
    public ProductKeyModel(LicenseModel licenseModel, int storeCode, string posId, int projectId, int expiredDate)
    {
        this.LicenseModel = licenseModel;
        this.StoreCode = storeCode;
        this.PosID = posId;
        this.ProjectId = projectId;
        this.ExpiredDate = ExpiredDate;
    }

    public ProductKeyModel(string applicationId, int totalStore, int storeCode, string posId, int projectId, int expiredDate)
    {
        this.LicenseModel = new LicenseModel(applicationId, totalStore);
        this.StoreCode = storeCode;
        this.PosID = posId;
        this.ProjectId = projectId;
        this.ExpiredDate = expiredDate;
    }
    #endregion

    #region Methods
    private string GenerateKey()
    {
        string result = string.Empty;
        if (LicenseModel != null)
        {
            string keyAfterGen = string.Format("{0}|{1}|{2}|{3}|{4}", LicenseModel.LisenceKey, StoreCode, PosID, ProjectId, ExpiredDate);
            //Encrypt string with ApplicationId is key unlock
            result = EncryptDecryptString.Encrypt(keyAfterGen, ProductKeyGenerator.ToNumbericValue(LicenseModel.ApplicationId));
        }

        return result;
    } 
    #endregion

}
