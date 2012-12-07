﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FlashCard.Database;
using FlashCard.Database.Repository;
using log4net;
using Microsoft.Shell;
using FlashCard.Views;


namespace FlashCard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        #region Properties
        public static LessonManageView LessonMangeView;
        private List<LessonModel> LessonCollection;
        private static ILog log;

        /// <summary>
        /// Variable check if user click on file & file valid, set IsStatupRunOk = true . not run method run normal
        /// </summary>
        private bool IsStatupRunOk = false;
        
        #endregion

        #region Startup Methods
        [STAThread]
        public static void Main()
        {
            var currentFolder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            var currentUserName = Environment.UserName;
            log4net.GlobalContext.Properties["LogName"] = String.Format("{0}/FlashCardLogs/{1}-{2}.log", currentFolder,currentUserName, DateTime.Now.ToString("yyyyMMdd"));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info(string.Empty);
            log.Info(" ======= Flash card Run=====");
            try
            {
                string appName = System.Configuration.ConfigurationManager.AppSettings["ApplicationName"];
                log.Info("appName OK");
                //string appName = "SmardFashCard";
                if (SingleInstance<App>.InitializeAsFirstInstance(appName))
                {
                    log.Info("Pass SingleInstance");
                    SetupRepository setupRepository = new SetupRepository();
                    var setup = setupRepository.GetAll<Setup>();
                    log.Info("setupRepository.GetAll");
                    if (setup.Count == 0)
                        CacheObject.Add<SetupModel>("SetupModel",new SetupModel()) ;
                    else
                        CacheObject.Add<SetupModel>( "SetupModel",new SetupModel(setup.FirstOrDefault()));

                    StudyRepository studyRespository = new StudyRepository();
                    var study = studyRespository.GetAll<Study>();
                    log.Info("studyRespository.GetAll<Study>()");
                    if (study != null)
                        CacheObject.Add<StudyModel>("StudyModel",new StudyModel(study.FirstOrDefault()));
                    else
                        CacheObject.Add<StudyModel>("StudyModel",new StudyModel());
                    var application = new App();
                    application.InitializeComponent();

                    application.Run();
                    // Allow single instance code to perform cleanup operations
                    SingleInstance<App>.Cleanup();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // @"E:\Desktop\3000 General Word.flc"
            // @"E:\Desktop\test.txt"
            //var file = @"E:\Desktop\Sorable.fcard";
            log.Info("OnStartup Run");
            try
            {
                //var file = e.Args.FirstOrDefault().ToString();
                var file = @"E:\Desktop\Toeic A6.fcard";
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                if (fileInfo.Exists && ".fcard".Equals(fileInfo.Extension))
                {
                    CacheObject.Get<SetupModel>("SetupModel").IsRunStatup = true;
                    var card = Serializer<CardModel>.Deserialize(file);
                    
                    if (card != null)
                    {
                        card.ToEntity();
                        foreach (var item in card.LessonCollection)
                        {
                            item.Lesson.Card = card.Card;
                            item.ToEntity();
                            
                        }
                        LessonCollection = card.LessonCollection.ToList();
                        if (LessonCollection != null && LessonCollection.Count() > 0 && CacheObject.Get<SetupModel>("SetupModel").IsRunStatup)
                        {
                            LessonMangeView = new LessonManageView(this.LessonCollection, true);
                            IsStatupRunOk = true;
                            LessonMangeView.Show();
                            
                        }
                    }
                }
                else
                {
                    MessageBox.Show("File not valid", "Error", MessageBoxButton.OK);
                }
            }
            catch (NullReferenceException NullEx)
            {
                log.Info(NullEx);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            //ImportFromSite view = new ImportFromSite();
            //view.Show();
            if (!IsStatupRunOk)
            {
                log.Info("RunNormal()");
                RunNormal();
            }

            base.OnStartup(e);
        }

        public App()
        {

        } 
        #endregion

        #region Methods
        /// <summary>
        /// Methods execute when user click on file but not valid or has exception with file
        /// </summary>
        private void RunNormal()
        {
            if (CacheObject.Get<SetupModel>("SetupModel").IsOpenLastStudy == true)
            {
                log.Info("SetupModel.IsOpenLastStudy==true");
                var lesson = CacheObject.Get<StudyModel>("StudyModel").Study.StudyDetails.Where(x => x.IsLastStudy == true).Select(x => x.Lesson).Distinct();
                LessonCollection = new List<LessonModel>();
                LessonCollection.AddRange(lesson.Select(x => new LessonModel(x)));
                LessonMangeView = new LessonManageView(this.LessonCollection, false);
            }
            else
            {
                log.Info("SetupModel.IsOpenLastStudy==false");
                LessonMangeView = new LessonManageView();
                LessonMangeView.Show();
            }
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
