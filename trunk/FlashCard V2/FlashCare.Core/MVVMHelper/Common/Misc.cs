using System;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;

public class Misc
{
    public readonly static string AssemblyFolder = Application.ResourceAssembly.Location.Substring(0,
            Application.ResourceAssembly.Location.LastIndexOf(@"\") + 1);

    #region Static fields
    public static bool IsFirstRunApp = false;
    public static bool CanConnectDB = false;
    public static bool IsDeveloping = true;
    public static readonly bool IsInstalled = true;

    //public static readonly string Key = "CPC";

    public static readonly string RememberPath = System.IO.Path.GetTempPath() + "login.cpc";
    public static readonly string StandardUnit = "each";
    #endregion

    #region Configuration
    /// <summary>
    /// Generatate new file name
    /// </summary>
    /// <returns>new name</returns>
    public string GenerateFilename()
    {
        return string.Format("Logo{0}", DateTimeExt.Now.ToString("yyyyMMddhhmmss"));
    }

    #endregion

}