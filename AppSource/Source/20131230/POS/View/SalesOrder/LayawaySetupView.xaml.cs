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

namespace CPC.POS.View
{
	/// <summary>
    /// Interaction logic for LayawaySetupView.xaml
	/// </summary>
	public partial class LayawaySetupView
	{
        public LayawaySetupView()
		{
			this.InitializeComponent();
            btnNew.Click += (s, e) =>
            {
                txtName.Focus();
            };
		}
	}
}