using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Helper
{
    public static class StringHelper
    {
        /// <summary>
        /// Clean Invalid Char in file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CleanFileName(string fileName)
        {
            return System.IO.Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty)).Trim();
        }
    }
}
