using System;
using System.Collections.Generic;
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
using TimePicker;
using System.ComponentModel;
using System.Linq;
namespace Tims.View.Employee
{
	/// <summary>
	/// Interaction logic for ScheduleManagement.xaml
	/// </summary>
	public partial class ViewWorkSchedule
	{
        public ViewWorkSchedule()
		{
			this.InitializeComponent();
            this.btnClose1.Click += new RoutedEventHandler(btnClose1_Click);
		}

        void btnClose1_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive == true).Close();
        }
	}
}