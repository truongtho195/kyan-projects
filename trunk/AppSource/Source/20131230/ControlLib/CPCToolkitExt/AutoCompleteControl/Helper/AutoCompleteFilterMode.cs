using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPCToolkitExt
{

    public enum DiscountType
    {
        Currency = 0,
        Percent = 1
    }

    public enum AutoCompleteFilterMode
    {
     
        /// <summary>
        /// Specifies a culture-sensitive, case-insensitive filter where the
        /// returned items start with the specified text. The filter uses the
        /// <see cref="M:System.String.StartsWith(System.String,System.StringComparison)" />
        /// method, specifying
        /// <see cref="P:System.StringComparer.CurrentCultureIgnoreCase" /> as
        /// the string comparison criteria.
        /// </summary>
        StartsWith = 0,

        /// <summary>
        /// Specifies a culture-sensitive, case-insensitive filter where the
        /// returned items contain the specified text.
        /// </summary>
        Contains = 1,

        /// <summary>
        /// Specifies a culture-sensitive, case-sensitive filter where the
        /// returned items contain the specified text.
        /// </summary>

        /// <summary>
        /// Specifies a culture-sensitive, case-insensitive filter where the
        /// returned items equal the specified text. The filter uses the
        /// <see cref="M:System.String.Equals(System.String,System.StringComparison)" />
        /// method, specifying
        /// <see cref="P:System.StringComparer.CurrentCultureIgnoreCase" /> as
        /// the search comparison criteria.
        /// </summary>
        Equals = 2
    }
}
