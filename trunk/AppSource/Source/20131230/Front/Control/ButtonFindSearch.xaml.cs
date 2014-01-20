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

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for ButtonFindSearch.xaml
    /// </summary>
    public partial class ButtonFindSearch : UserControl
    {
        #region Properties
        public double LabelWidth
        {
            get { return stkTitle.Width; }
            set
            {
                if (stkTitle.Width != value)
                {
                    stkTitle.Width = value;
                }
            }
        }

        public double TextWidth
        {
            get { return txtSearch.Width; }
            set
            {
                if (txtSearch.Width != value)
                {
                    txtSearch.Width = value;
                }
            }
        }
        #endregion

        #region Constructors
        public ButtonFindSearch()
        {
            this.InitializeComponent();
            this.txtSearch.PreviewKeyDown += new KeyEventHandler(txtSearch_PreviewKeyDown);
            this.txtSearch.KeyDown += new KeyEventHandler(txtSearch_KeyDown);
        }
        #endregion

        #region Events
        /// <summary>
        /// Event when user press Up or Down button to change the SearchSelect of the combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool? isUp = null;
            if (e.Key == Key.Up)
                isUp = true;
            else if (e.Key == Key.Down)
                isUp = false;
            if (isUp != null && SearchList != null)
            {
                int selectIndex = SearchList.IndexOf(SearchSelected);
                //change the SearchSelected belong to Up or Down
                int newIndex = isUp.Value ? selectIndex - 1 : selectIndex + 1;
                //out of range Index - then return
                if (newIndex <= SearchList.Count - 1 && newIndex >= 0)
                    SearchSelected = SearchList[newIndex];
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.btnFind.Command != null)
                    this.btnFind.Command.Execute(this.btnFind.CommandParameter);
            }
        }
        #endregion

        #region Dependency properties

        #region SearchList - collection for the searchcombo box

        public IList<string> SearchList
        {
            get { return (IList<string>)GetValue(SearchListProperty); }
            set { SetValue(SearchListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchListProperty =
            DependencyProperty.Register("SearchList", typeof(IList<string>), typeof(ButtonFindSearch));

        #endregion

        #region SearchSelected

        public string SearchSelected
        {
            get { return (string)GetValue(SearchSelectedProperty); }
            set { SetValue(SearchSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchSelectedProperty =
            DependencyProperty.Register("SearchSelected", typeof(string), typeof(ButtonFindSearch));

        #endregion

        #region SearchString


        public string SearchString
        {
            get { return (string)GetValue(SearchStringProperty); }
            set { SetValue(SearchStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchStringProperty =
            DependencyProperty.Register("SearchString", typeof(string), typeof(ButtonFindSearch));


        #endregion

        #region Command for SearchingCommand

        public ICommand SearchingCommand
        {
            get { return (ICommand)GetValue(SearchingCommandProperty); }
            set { SetValue(SearchingCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchingCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchingCommandProperty =
            DependencyProperty.Register("SearchingCommand", typeof(ICommand), typeof(ButtonFindSearch));

        #endregion

        #endregion
    }
}