using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CPC.ViewModel;

namespace CPC.TimeClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);
            this.KeyUp += new System.Windows.Input.KeyEventHandler(MainWindow_KeyUp);
        }

        private void MainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string eKey = e.Key.ToString();
            Console.WriteLine(eKey);
            if (eKey.Length == 1)
                txtBarCode.Text += eKey;
            else if (eKey.Length == 2 && eKey.StartsWith("D"))
                // Don't accept number input from numpad
                txtBarCode.Text += eKey.Substring(1);
            else if (eKey.StartsWith("NumPad"))
                txtBarCode.Text += eKey.Substring(6);

            Console.WriteLine(txtBarCode.Text);

            if (e.Key == Key.Enter)
            {
                BindingExpression bindingExpression = txtBarCode.GetBindingExpression(TextBox.TextProperty);
                bindingExpression.UpdateSource();
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(txtBarCode.Text);
            if (txtBarCode.Text.Equals("Refresh"))
            {
                (DataContext as MainViewModel).RefreshDatas();
                txtBarCode.Clear();
            }
            else
            {
                BindingExpression bindingExpression = txtBarCode.GetBindingExpression(TextBox.TextProperty);
                bindingExpression.UpdateSource();
            }
        }
    }
}