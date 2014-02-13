using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CPC.POS.ViewModel.Synchronization
{
    public static class CreateFile
    {
        public static int ID = 0;

        public static string Name;

        public static string Path = Application.ResourceAssembly.Location.Substring(0, Application.ResourceAssembly.Location.LastIndexOf(@"\") + 1) + string.Format("{0}_Log.txt", Name);

        public static void WriteText(string content)
        {
            string contentstart = string.Format("--{0}--", DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"));
            string contentend = "--";
            string format = string.Format("{0}{1}{2}\n", contentstart, content, contentend);
            // Write the string to a file.
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path, true))
            {
                file.WriteLine(format);
            }
            ID++;
        }
    }
}
