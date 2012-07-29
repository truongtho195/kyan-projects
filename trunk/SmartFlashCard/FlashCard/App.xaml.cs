using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FlashCard.Database;
using FlashCard.Database.Repository;
using log4net;
using Microsoft.Shell;


namespace FlashCard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        #region Properties
        public static LessonManageView LessonMangeView;
        public static SetupModel SetupModel;
        public static StudyModel StudyModel;


        private List<LessonModel> LessonCollection;
        private static ILog log;

        /// <summary>
        /// Variable check if user click on file & file valid, set IsStatupRunOk = true . not run method run normal
        /// </summary>
        private bool IsStatupRunOk = false;
        #endregion

        [STAThread]
        public static void Main()
        {
            string appName = System.Configuration.ConfigurationManager.AppSettings["ApplicationName"];

            //string appName = "SmardFashCard";
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

                StudyRepository studyRespository = new StudyRepository();
                var study = studyRespository.GetAll<Study>();
                if (study != null)
                    App.StudyModel = new StudyModel(study.FirstOrDefault());
                else
                    App.StudyModel = new StudyModel();
                var application = new App();
                application.InitializeComponent();

                application.Run();
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // @"E:\Desktop\3000 General Word.flc"
            // @"E:\Desktop\test.txt"
            //var file = @"E:\Desktop\Sorable.fcard";
            try
            {
                var file = e.Args.FirstOrDefault().ToString();
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                if (fileInfo.Exists && ".fcard".Equals(fileInfo.Extension))
                {
                    SetupModel.IsRunStatup = true;
                    var card = Serializer<Card>.Deserialize(file);
                    if (card != null)
                    {
                        LessonCollection = new List<LessonModel>(card.Lessons.Select(x => new LessonModel(x)));
                        if (LessonCollection != null && LessonCollection.Count() > 0 && SetupModel.IsRunStatup)
                        {
                            LessonMangeView = new LessonManageView(this.LessonCollection, true);
                            IsStatupRunOk = true;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("File not valid", "Error", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            if (!IsStatupRunOk)
            {
                RunNormal();
            }

            base.OnStartup(e);
        }


        public App()
        {
            var currentUserName = Environment.UserName;
            log4net.GlobalContext.Properties["LogName"] = String.Format("FlashCardLogs/{0}-{1}.log", currentUserName, DateTime.Now.ToString("yyyyMMdd"));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info(string.Empty);
            log.Info(" ======= Flash card Run=====");
        }



        #region Methods
        /// <summary>
        /// Methods execute when user click on file but not valid or has exception with file
        /// </summary>
        private void RunNormal()
        {
            if (SetupModel.IsOpenLastStudy == true)
            {
                var lesson = App.StudyModel.Study.StudyDetails.Where(x => x.IsLastStudy == true).Select(x => x.Lesson).Distinct();
                LessonCollection = new List<LessonModel>();
                LessonCollection.AddRange(lesson.Select(x => new LessonModel(x)));
                LessonMangeView = new LessonManageView(this.LessonCollection, false);
            }
            else
            {
                LessonMangeView = new LessonManageView();
            }
            LessonMangeView.Show();
        }

        public bool SignalExternalCommandLineArgs(System.Collections.Generic.IList<string> args)
        {
            return true;
        }

        #endregion

        #region Handle program Exception

        protected void FlashCardDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
            log.Error(e.Exception);
            e.Handled = true;
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private static void HandleException(Exception e, bool isTerminating)
        {
            if (e == null) { return; }

            log.Error(e);

            if (!isTerminating)
            {
                // show the message to the user
            }
        }

        #endregion Handle program Exception
    }
}
