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
using System.Collections.ObjectModel;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for FilterAdvanceSearch.xaml
    /// </summary>
    public partial class FilterAdvanceSearch : UserControl
    {

        #region Properties

        public object Params
        {
            get { return (object)GetValue(ParamsProperty); }
            set { SetValue(ParamsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Params.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParamsProperty =
            DependencyProperty.Register("Params", typeof(object), typeof(FilterAdvanceSearch), new UIPropertyMetadata(null, new PropertyChangedCallback(FilterAdvanceSearch.OnParamsChanged)));

        private static void OnParamsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //(sender as FilterAdvanceSearch).Params = e.NewValue;
        }

        #endregion

        #region Dependency Properties

        #region Switch Mode
        public bool IsAdvanceMode
        {
            get { return (bool)GetValue(IsAdvanceModeProperty); }
            set { SetValue(IsAdvanceModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAdvancedMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAdvanceModeProperty =
            DependencyProperty.Register("IsAdvanceMode", typeof(bool), typeof(FilterAdvanceSearch), new UIPropertyMetadata(false));
        #endregion

        #region Simple Mode
        public string FilterText
        {
            get { return (string)GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.Register("FilterText", typeof(string), typeof(FilterAdvanceSearch),
            new UIPropertyMetadata(String.Empty)); 
        #endregion

        #region Advance Mode
        public int? ItemsCount
        {
            get { return (int?)GetValue(ItemsCountProperty); }
            set { SetValue(ItemsCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsCountProperty =
            DependencyProperty.Register("ItemsCount", typeof(int?), typeof(FilterAdvanceSearch), new UIPropertyMetadata(null));

        public ObservableCollection<object> ItemsSource
        {
            get { return (ObservableCollection<object>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        } 
        
        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<object>), typeof(FilterAdvanceSearch),
            new UIPropertyMetadata(null));
        #endregion

        #region Command

        // FilterCommand

        public ICommand FilterCommand
        {
            get { return (ICommand)GetValue(FilterCommandProperty); }
            set { SetValue(FilterCommandProperty, value); }
        }
        public static readonly DependencyProperty FilterCommandProperty =
            DependencyProperty.Register("FilterCommand", typeof(ICommand), typeof(FilterAdvanceSearch),
            new FrameworkPropertyMetadata(null,
                new PropertyChangedCallback(FilterAdvanceSearch.OnCommandChanged)));

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as FilterAdvanceSearch).ctlAdvanceSearch.Command = (e.NewValue as ICommand);
        }

        #endregion

        #endregion

        #region Constructors

        public FilterAdvanceSearch()
        {
            this.InitializeComponent();

            // IsAdvanceMode = false
            this.txtFilterText.Text = String.Empty;
            Binding binding = new Binding
            {
                Source = this,
                Path = new PropertyPath("FilterText"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            this.SetBinding(FilterAdvanceSearch.ParamsProperty, binding);
        }

        #endregion

        #region Events

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (IsAdvanceMode)
            {
                this.ctlAdvanceSearch.Focus();
            }
            else
            {
                this.txtFilterText.Focus();
            }

            base.OnGotFocus(e);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            this.txtblFilterText.Visibility = System.Windows.Visibility.Collapsed;
            this.txtFilterText.Visibility = System.Windows.Visibility.Collapsed;
            this.ctlAdvanceSearch.Visibility = System.Windows.Visibility.Visible;
            this.txtblSimpleFilter.Visibility = System.Windows.Visibility.Visible;
            this.txtblAdvanceFilter.Visibility = System.Windows.Visibility.Collapsed;

            IsAdvanceMode = true;

            //BindingOperations.ClearBinding(this, FilterAdvanceSearch.FilterTextProperty);
            Binding binding1 = new Binding
            {
                Source = this.ctlAdvanceSearch,
                Path = new PropertyPath("SelectedItems"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            this.SetBinding(FilterAdvanceSearch.ParamsProperty, binding1);

            this.txtFilterText.Text = String.Empty;
            this.ctlAdvanceSearch.ClearData();
            this.ctlAdvanceSearch.Filter();

            //Hyperlink hyperLink = sender as Hyperlink;
            //Run run = new Run("Simple Search");
            //hyperLink.Inlines.Clear();
            //hyperLink.Inlines.Add(run);

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.txtblFilterText.Visibility = System.Windows.Visibility.Visible;
            this.txtFilterText.Visibility = System.Windows.Visibility.Visible;
            this.ctlAdvanceSearch.Visibility = System.Windows.Visibility.Collapsed;
            this.txtblSimpleFilter.Visibility = System.Windows.Visibility.Collapsed;
            this.txtblAdvanceFilter.Visibility = System.Windows.Visibility.Visible;

            IsAdvanceMode = false;

            this.txtFilterText.Text = String.Empty;

            //BindingOperations.ClearBinding(this.ctlAdvanceSearch, Tims.Control.FilterAdvanceView.SelectedItemsProperty);
            Binding binding = new Binding
            {
                Source = this,
                Path = new PropertyPath("FilterText"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            this.SetBinding(FilterAdvanceSearch.ParamsProperty, binding);

            this.ctlAdvanceSearch.FilterText();
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            this.ctlAdvanceSearch.Filter();
        }

        #endregion

    }
}