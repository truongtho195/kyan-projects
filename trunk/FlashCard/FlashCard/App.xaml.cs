using System;
using System.Windows;
using FlashCard.Model;
using Microsoft.Shell;

namespace FlashCard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        [STAThread]
        public static void Main()
        {
            //string appName = System.Configuration.ConfigurationManager.AppSettings["ApplicationName"];
            string appName = "FashCard";
            if (SingleInstance<App>.InitializeAsFirstInstance(appName))
            {
                var application = new App();
                application.InitializeComponent();

                application.Run();
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        public App()
        {
            string filePath = "FlashCardLog-" +DateTime.Now.ToString("yyyyMMdd") + ".txt";
            log4net.GlobalContext.Properties["LogName"] = filePath;


            LessonMangeView = new LessonManageView();
            LessonMangeView.Show();
        }
        public static SetupModel SetupModel;
        public static LessonManageView LessonMangeView;

        public bool SignalExternalCommandLineArgs(System.Collections.Generic.IList<string> args)
        {
            return true;
        }

        #region Handle program Exception

        protected void FlashCardDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
            e.Handled = true;
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private static void HandleException(Exception e, bool isTerminating)
        {
            if (e == null) { return; }

            //log.Error(e);

            if (!isTerminating)
            {
                // show the message to the user
            }
        }

        #endregion Handle program Exception
    }
}
