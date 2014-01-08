using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


class MachineUtility
{
    #region Times
    /// <summary>
    /// Get Last Bootup Time
    /// </summary>
    /// <returns></returns>
    public static string LastBootupTime()
    {
        string result = string.Empty;

        SelectQuery query = new SelectQuery("SELECT LastBootUpTime FROM Win32_OperatingSystem WHERE Primary='true'");
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
        foreach (ManagementObject mo in searcher.Get())
        {
            DateTime dt =
            ManagementDateTimeConverter.ToDateTime(mo.Properties["LastBootUpTime"].Value.ToString());

            result += dt;
        }
        return result;
    }


    /// <summary>
    /// method to get the current systems up-time, returned in a Timespan value
    /// </summary>
    /// <returns></returns>
    public static TimeSpan GetSystemUptime()
    {
        //timespan object to store the result value
        TimeSpan up = new TimeSpan();

        //management objects to interact with WMI 
        //Require Using System.Management;
        ManagementClass m = new ManagementClass("Win32_OperatingSystem");

        //loop throught the WMI instances
        foreach (ManagementObject instance in m.GetInstances())
        {
            //get the LastBootUpTime date parsed (comes in CIM_DATETIME format)
            DateTime last = ManagementDateTimeConverter.ToDateTime(instance["LastBootUpTime"].ToString());

            //check it value is not DateTime.MinValue
            if (last != DateTime.MinValue)
                //get the diff between dates
                up = DateTime.Now - last;
        }

        //return the uptime TimeSpan
        return up;
    }
    #endregion

    #region Physical Memory
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        public MEMORYSTATUSEX()
        {
            this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    public static ulong GetTotalRam()
    {
        MEMORYSTATUSEX mem = new MEMORYSTATUSEX();

        GlobalMemoryStatusEx(mem);

        return mem.ullTotalPhys;
    }

    public static ulong GetFreeRam()
    {
        MEMORYSTATUSEX mem = new MEMORYSTATUSEX();

        GlobalMemoryStatusEx(mem);

        return mem.ullAvailPhys;
    }
    #endregion

    #region HardDisk
    private static void GetHardDisk()
    {
        DriveInfo[] allDrives = DriveInfo.GetDrives();

        foreach (DriveInfo d in allDrives)
        {
            Console.WriteLine("Drive {0}", d.Name);
            Console.WriteLine("  File type: {0}", d.DriveType);
            if (d.IsReady == true)
            {
                Console.WriteLine("  Volume label: {0}", d.VolumeLabel);
                Console.WriteLine("  File system: {0}", d.DriveFormat);
                Console.WriteLine(
                    "  Available space to current user:{0, 15} bytes",
                    d.AvailableFreeSpace);

                Console.WriteLine(
                    "  Total available space:          {0, 15} bytes",
                    d.TotalFreeSpace);

                Console.WriteLine(
                    "  Total size of drive:            {0, 15} bytes ",
                    d.TotalSize);
            }
        }
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

    public static string GetHDDVolumnSerial(string drive)
    {
        ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
        dsk.Get();
        return dsk["VolumeSerialNumber"].ToString();
    }

    /// <summary>
    /// Get Partition System
    /// </summary>
    /// <returns></returns>
    public static string GetPrimaryVolumneSerialNumber()
    {
        ManagementClass partionsClass = new ManagementClass("Win32_LogicalDisk");
        ManagementScope scope = new ManagementScope("root\\CIMV2");
        ManagementObjectCollection partions = partionsClass.GetInstances();

        partionsClass.Scope = scope;
        string strDiskSerial = string.Empty;
        
        foreach (ManagementObject partion in partions)
        {
            strDiskSerial = Convert.ToString(partion["VolumeSerialNumber"]);
            break;
        }
        return strDiskSerial;
    }
    #endregion

    #region UniqueID
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

   
    #endregion

    #region NetworkUtil
    public static string GetHostName()
    {
        return Dns.GetHostName();
    }

    public static string GetIpAddress()
    {
        string localIP = string.Empty;
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
            }
        }
        return localIP;
    }

    /// <summary>
    /// Get Free Port
    /// </summary>
    /// <returns></returns>
    public static int GetFreePort()
    {
        var listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public bool TryParseAddress(string address, out IPAddress ip)
    {
        if (IPAddress.TryParse(address, out ip))
            return true;
        else
        {
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(address);
                if (addresses.Any())
                {
                    ip = addresses.First();
                    return true;
                }
            }
            catch (SocketException) { }
        }
        return false;
    }

    #endregion
}
