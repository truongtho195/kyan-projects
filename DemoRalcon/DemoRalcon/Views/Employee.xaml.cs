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
using System.Windows.Shapes;

namespace DemoRalcon.Views
{
    /// <summary>
    /// Interaction logic for Employee.xaml
    /// </summary>
    public partial class Employee : Window
    {
        public Employee()
        {
            InitializeComponent();
            for (int i = 0; i < 100; i++)
            {
                EmployeeModel em = new EmployeeModel();
                em.FirstName = string.Format("FirstName {0}", i);
                em.LastName = string.Format("LastName {0}", i);
                em.MiddelName = string.Format("MiddelName {0}", i);
                list.Add(em);
            }
            this.dgEmployee.ItemsSource = list;
        }
        List<EmployeeModel> list = new List<EmployeeModel>();
        
    }

    public class EmployeeModel
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddelName { get; set; }
    }
}
