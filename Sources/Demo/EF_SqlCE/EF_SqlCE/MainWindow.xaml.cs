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

            try
            {
                AddBlog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }


        public static string getConStrSQL()
        {
            string linkFIle = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\FalconHRDB.s3db";
            string connectionString = new System.Data.EntityClient.EntityConnectionStringBuilder
            {
                Metadata = "res://*/Database.Model1.csdl|res://*/Database.Model1.ssdl|res://*/Database.Model1.msl",
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = new System.Data.SqlClient.SqlConnectionStringBuilder
                {
                    DataSource = linkFIle,
                    IntegratedSecurity = false

                }.ConnectionString
            }.ConnectionString;

            return connectionString;
        }

        //private void AddForFlashCard()
        //{
        //    SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
        //    flashCardEntity.Users.AddObject(new User() { FullName = "Hunter", Password = "654321", UserName = "hunter" });
        //    flashCardEntity.SaveChanges();
        //}



        //private void AddCateForNorthWind()
        //{
        //    NorthwindEntities entity = new NorthwindEntities();
        //    entity.Categories.AddObject(new Category() { Category_Name = "Cate of Kyan" });
        //    entity.SaveChanges();
        //}

        private void AddBlog()
        {
            BlogEngineDBEntities entity = new BlogEngineDBEntities();
            entity.Blogs.AddObject(new Blog()
                            {
                                BlogName = "my blog"

                            });
            entity.SaveChanges();
        }
    }
}
