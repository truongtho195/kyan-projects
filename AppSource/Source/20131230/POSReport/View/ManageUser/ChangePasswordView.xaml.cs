
using POSReport;
namespace CPC.POSReport.View
{
	/// <summary>
    /// Interaction logic for ChangePasswordView.xaml
	/// </summary>
	public partial class ChangePasswordView
	{
        public ChangePasswordView()
		{
			this.InitializeComponent();
		}

        public void ShowDialog(ChangePasswordView changPwdView)
        {
            this.ShowInTaskbar = false;
            this.Owner = App.Current.MainWindow;
            this.DataContext = new ViewModel.ChangePwdViewModel(changPwdView);
            this.ShowDialog();
        }

        private void BrdTopBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
	}
}