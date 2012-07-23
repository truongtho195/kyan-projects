using System;
using System.Windows;
using Microsoft.Shell;
using System.Linq;
using FlashCard.Views;
using FlashCard.Database;
using FlashCard.Database.Repository;
using System.Collections.Generic;


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


            string appName = "SmardFashCard";
            if (SingleInstance<App>.InitializeAsFirstInstance(appName))
            {
                SetupRepository setupRepository = new SetupRepository();
                var setup = setupRepository.GetAll<Setup>();
                if (setup.Count == 0)
                    SetupModel = new SetupModel();
                else
                {
                    SetupModel = new SetupModel(setup.FirstOrDefault());
                }


                var application = new App();
                application.InitializeComponent();

                application.Run();
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }
        private bool IsStatupRunOk = false;
        protected override void OnStartup(StartupEventArgs e)
        {
            // @"E:\Desktop\3000 General Word.flc"
            // @"E:\Desktop\test.txt"
            try
            {
                var file = e.Args.FirstOrDefault().ToString();
                //var file = @"E:\Desktop\test.txt";
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                if (fileInfo.Exists)
                {
                    SetupModel.IsRunStatup = true;
                    var card = Serializer<Card>.Deserialize(file);
                    if (card != null)
                    {
                        LessonCollection = new List<LessonModel>(card.Lessons.Select(x => new LessonModel(x)));
                        if (LessonCollection != null && LessonCollection.Count() > 0 && SetupModel.IsRunStatup)
                        {
                            LessonMangeView = new LessonManageView(this.LessonCollection);
                            IsStatupRunOk = true;
                        }
                    }
                   
                }
            }
            catch (Exception ex)
            {
                RunNormal();
            }
            if (!IsStatupRunOk)
            {
                RunNormal();
            }

            base.OnStartup(e);
        }

        private static void RunNormal()
        {
            var currentUserName = Environment.UserName;
            log4net.GlobalContext.Properties["LogName"] = String.Format("CardLog-{0}-{1}.txt", currentUserName, DateTime.Now.ToString("yyyyMMdd"));
            LessonMangeView = new LessonManageView();
            LessonMangeView.Show();
        }

        public static LessonManageView LessonMangeView;
        public static SetupModel SetupModel;

        private List<LessonModel> LessonCollection;
        public App()
        {
           
        }

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
