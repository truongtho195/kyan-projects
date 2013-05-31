using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using CPC.POS;
using CPC.POS.View;
using CPC.POS.ViewModel;

namespace CPC.Helper
{
    class Program
    {
        /// <summary>
        /// Enfore the application has only one instance
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // Check singleton app
            if (SingleInstance<App>.InitializeAsFirstInstance(Define.programName))
            {
                // Run splash screen
                SplashScreenView splashScreenView = new SplashScreenView();
                splashScreenView.Loaded += (sender, e) =>
                {
                    splashScreenView.DataContext = new SplashScreenViewModel(splashScreenView);
                };
                if (splashScreenView.ShowDialog() == false)
                    return;

                App application = new App();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
            else
            {
                // Get current process
                Process currentProcess = Process.GetCurrentProcess();

                // Get opened process
                Process myProcess = Process.GetProcessesByName(currentProcess.ProcessName).FirstOrDefault();

                // Active process if process is opened
                if (myProcess != null)
                    SetForegroundWindow(currentProcess.MainWindowHandle);
            }
        }

        [DllImportAttribute("User32.dll")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);
    }
}