using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml.Linq;
using System.IO;

/// <summary>
/// String convention
/// </summary>
static class StringUtils
{
    public static string ToLowercaseNamingConvention(this string s, bool toLowercase)
    {
        if (toLowercase)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            return r.Replace(s, "_").ToLower();
        }
        else
            return s;
    }

    public static string SplitCamelCase(this string str)
    {
        return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
    }

    public static string RemoveSpecialCharacters(this string input)
    {
        Regex r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        return r.Replace(input, String.Empty);
    }

    public static string RemoveMarks(this string input)
    {
        Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
        string strFormD = input.Normalize(System.Text.NormalizationForm.FormD);
        return regex.Replace(strFormD, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
    }

    public static string CamelCase(this string input)
    {
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
    }
}
