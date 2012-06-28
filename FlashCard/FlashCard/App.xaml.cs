using System;
using System.Windows;
using FlashCard.Model;

namespace FlashCard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            string filePath = "FlashCardLog-" +DateTime.Now.ToString("yyyyMMdd") + ".txt";
            log4net.GlobalContext.Properties["LogName"] = filePath;
        }
        public static SetupModel SetupModel;


    }
}
