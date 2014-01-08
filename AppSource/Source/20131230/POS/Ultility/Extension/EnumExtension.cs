using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;


static class EnumExtension
{
    public static string ToDescription(this Enum value)
    {
        var da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
        return da.Length > 0 ? da[0].Description : value.ToString();
    }
}
