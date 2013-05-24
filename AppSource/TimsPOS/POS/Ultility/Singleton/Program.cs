using System;
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
        }
    }
}