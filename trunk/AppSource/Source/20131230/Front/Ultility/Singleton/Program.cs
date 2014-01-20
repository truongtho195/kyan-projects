using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using CPC.POS;
using CPC.POS.View;
using CPC.POS.ViewModel;
using System.Windows;

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
                // Get opened process
                Process myProcess = Process.GetProcessesByName(Define.programName).FirstOrDefault(x => !x.MainWindowHandle.Equals(IntPtr.Zero));

                if (myProcess != null)
                {
                    // Show notification
                    Xceed.Wpf.Toolkit.MessageBox.Show("Already an instance is running...", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Active process if process is opened
                    SetForegroundWindow(myProcess.MainWindowHandle);
                }
            }
        }

        [DllImportAttribute("user32.dll")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);
    }
}