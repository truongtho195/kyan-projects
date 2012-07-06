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
using EF_SqlCE.Repository;
using EF_SqlCE.Database;
namespace EF_SqlCE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            BlogRepository blogRep = new BlogRepository();
            Blog blog = new Blog();
            blog.BloggerName = "Kyan Blog";
            blog.Title = "sharing to sharing";
            try
            {
                blogRep.Add(blog);

                var item = blogRep.Get();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
