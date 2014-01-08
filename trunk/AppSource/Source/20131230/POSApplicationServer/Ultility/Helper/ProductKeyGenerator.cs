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
        AddHardware();
    }
    #endregion

    #region Property
    Dictionary<string, string> hardWareCollection = new Dictionary<string, string>();
    #endregion

    #region Generate & Validate
    /// <summary>
    /// General Key
    /// </summary>
    /// <param name="input"></param>
    /// <param name="isFormated"></param>
    /// <returns></returns>
    public static string GenerateKey(string input, bool isFormated = false)
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
    /// Validate input Product key with options in machine
    /// </summary>
    /// <param name="productKey"></param>
    /// <param name="configModel"></param>
    /// <returns></returns>
    public bool ValidateProductKey(string productKey, LicenseModel licenseModel)
    {
        bool result = false;
        string clearProductKey = ClearString(productKey);

        LicenseModel licenseInfo = licenseModel;
        foreach (var item in hardWareCollection)
        {
            licenseInfo.ApplicationId = item.Value;
            string currentKeyGen = GenerateKey(licenseInfo.ToString());
            Console.WriteLine(currentKeyGen.ToLower());
            if (!string.IsNullOrWhiteSpace(clearProductKey) && clearProductKey.ToLower().Equals(currentKeyGen.ToLower()))
            {
                result = true;
                break;
            }
        }

        return result;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Default Hardware in Machine
    /// </summary>
    private void AddHardware()
    {
        hardWareCollection.Add("CPU", GetCpuId());
        hardWareCollection.Add("HDD", GetHddSerial());
        hardWareCollection.Add("MacAddress", GetMacAddress());
    }
    #endregion

    #region Helper
    public string ClearString(string input)
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
            Console.WriteLine(item);
            int i = (int)(item - '0');
            ouput += i.ToString();
        }
        return ouput;
    }

    /// <summary>
    /// string to hex
    /// </summary>
    /// <param name="inputString"></param>
    public static void ToHex(string inputString)
    {
        var as_hex = inputString.Select(x => ((int)x).ToString("X02"));
        Console.WriteLine("To Hex :{0}", string.Join("", as_hex));
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
                return wmi_HD["SerialNumber"].ToString();
        }
        return string.Empty;
    }


    #endregion
}

class LicenseModel
{
    public LicenseModel()
    {

    }

    public LicenseModel(string licenseName, string applicationId, int storeQty, DateTime expireDate)
    {
        this.LicenseName = licenseName;
        this.ApplicationId = applicationId;
        this.StoreQty = storeQty;
        this.ExpireDate = expireDate;

    }

    public string LicenseName { get; set; }
    public string ApplicationId { get; set; }
    public int StoreQty { get; set; }
    public DateTime ExpireDate { get; set; }

    public override string ToString()
    {
        return string.Format("{0}{1}{2}{3}{4}", LicenseName, ApplicationId, StoreQty, ExpireDate.Date);
    }
}
