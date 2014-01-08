using System.Windows;
using System.Windows.Controls;

namespace CPC.Control
{
    /// <summary>
    /// Control that contains two elements: a text box that contains
    /// the filter text, and a reset button that clears the filter text.
    /// The reset button is only visible when there is filter text.
    ///
    /// </summary>
    public partial class FilterText : UserControl
    {

        #region Dependency Properties
        /// <summary>
        /// Gets or sets the text content of the filter control.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FilterText), new UIPropertyMetadata(TextChangedCallback));

        private static void TextChangedCallback(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as FilterText).TextChanged();
        }

        #endregion

        #region Constructors
        public FilterText()
        {
            InitializeComponent();
            FilterTextBox.TextChanged += new TextChangedEventHandler(FilterTextBox_TextChanged);
            FilterButton.Click += new RoutedEventHandler(FilterButton_Click);
            ShowResetButton();
        }
        #endregion

        #region Events
        /// <summary>
        /// The reset button was clicked, clear the filter control.
        /// </summary>
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            SetValue(TextProperty, string.Empty);
            FilterTextBox.Focus();
            //RaiseEvent(new RoutedEventArgs(ResetFilterEvent));
        }

        /// <summary>
        /// The filter text changed, show or hide the reset button.
        /// </summary>
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowResetButton();
            SetValue(TextProperty, FilterTextBox.Text);
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Set the focus to the filter control.
        /// </summary>
        public new void Focus()
        {
            FilterTextBox.Focus();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Show the reset button if there is any text in the filter,
        /// otherwise hide the reset button.
        /// </summary>
        private void ShowResetButton()
        {
            FilterButton.Visibility = (FilterTextBox.Text.Trim().Length > 0) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        private void TextChanged()
        {
            FilterTextBox.Text = Text;
        }
        #endregion

    }
}
