using System;
using CPC.TimeClock;
using Microsoft.Shell;

namespace CPC.TimeClock
{
    class Program
    {
        /// <summary>
        /// Enfore the application has only one instance
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(define.programName))
            {
                var application = new App();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }
    }
}
