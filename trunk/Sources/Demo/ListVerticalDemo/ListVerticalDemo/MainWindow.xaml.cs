using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ListVerticalDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BackSideList.Add(new BackSideModel() { BackSideID = 1, BackSideName = "Back Side Name 1", Content = "Content" });
            BackSideList.Add(new BackSideModel() { BackSideID = 2, BackSideName = "Back Side Name 2", Content = "Content" });
            BackSideList.Add(new BackSideModel() { BackSideID = 3, BackSideName = "Back Side Name 3", Content = "Content" });
            BackSideList.Add(new BackSideModel() { BackSideID = 4, BackSideName = "Back Side Name 4", Content = "Content" });
            BackSideList.Add(new BackSideModel() { BackSideID = 5, BackSideName = "Back Side Name 5", Content = "Content" });
            BackSideList.Add(new BackSideModel() { BackSideID = 6, BackSideName = "Back Side Name 6", Content = "Content" });
            BackSideList.Add(new BackSideModel() { BackSideID = 7, BackSideName = "Back Side Name 7", Content = "Content" });
            BackSideList.Add(new BackSideModel() { BackSideID = 8, BackSideName = "Back Side Name 8", Content = "Content" });
            lstListBox.ItemsSource = BackSideList;
            lstListBox.SelectedIndex = 1;
        }
        private List<BackSideModel> BackSideList = new List<BackSideModel>();
    }

    internal class BackSideModel
    {
        public int BackSideID { get; set; }
        public string BackSideName { get; set; }
        public string Content { get; set; }
    }
}
