using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using FlashCard.Model;
using log4net;

namespace FlashCard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            string filePath = "FlashCardLog" + DateTime.Now + ".txt";
            log4net.GlobalContext.Properties["LogName"] = filePath;//String.Format("FlashCardLog{0}.txt",DateTime.Today);
        }
        public static SetupModel SetupModel;


    }
}
