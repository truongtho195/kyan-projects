using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPCToolkitExt
{
    /// <summary>
    /// A predefined set of filter functions for the known, built-in 
    /// AutoCompleteFilterMode enumeration values.
    /// </summary>
    internal static class AutoCompleteSearch
    {
        /// <summary>
        /// Index function that retrieves the filter for the provided 
        /// AutoCompleteFilterMode.
        /// </summary>
        /// <param name="FilterMode">The built-in search mode.</param>
        /// <returns>Returns the string-based comparison function.</returns>
        public static bool GetFilter(AutoCompleteFilterMode FilterMode, string text, string value)
        {
            switch (FilterMode)
            {
                case AutoCompleteFilterMode.Contains:
                    return Contains(text, value);

                case AutoCompleteFilterMode.Equals:
                    return Equals(text, value);

                case AutoCompleteFilterMode.StartsWith:
                    return StartsWith(text, value);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Check if the string value begins with the text.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool StartsWith(string text, string value)
        {
            return value.ToUpper().StartsWith(text.ToUpper(), StringComparison.CurrentCultureIgnoreCase);
        }
        /// <summary>
        /// Check if the prefix is contained in the string value. The current 
        /// culture's case insensitive string comparison operator is used.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool Contains(string text, string value)
        {
            return value.ToUpper().Contains(text.ToUpper());
        }

        /// <summary>
        /// Check if the string values are equal.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool Equals(string text, string value)
        {
            return value.ToUpper().Equals(text.ToUpper(), StringComparison.CurrentCultureIgnoreCase);
        }

    }

    public enum CPCToolkitExtPropertyType
    {
        Null = 0,
        Model = 1,
        Collection = 2
    }
}
