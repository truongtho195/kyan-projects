using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class LicenseModel
{
    public string LicenseName { get; set; }
    public string ApplicationId { get; set; }
    public int StoreQty { get; set; }
    public DateTime ExpireDate { get; set; }

    public override string ToString()
    {
        return string.Format("{0}{1}{2}{3}{4}", LicenseName, ApplicationId, StoreQty, ExpireDate);
    }
    
}

