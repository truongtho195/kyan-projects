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
using System.Windows.Media.Animation;
using CPC.POS;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for RunningTextControl.xaml
    /// </summary>
    public partial class RunningTextControl : UserControl
    {
        #region Ctor
        public RunningTextControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(RunningTextControl_Loaded);
        }
        #endregion

        #region Event
        private void RunningTextControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded && this.IsVisible)
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.From = -tbmarquee.ActualWidth;
                canMain.Width = this.ActualWidth;
                doubleAnimation.To = canMain.Width;
                doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
                doubleAnimation.Duration = new Duration(TimeSpan.Parse(string.Format("0:0:{0}", this.Duration)));
                tbmarquee.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
            }
        }
        #endregion

        #region Dependency Properties
     
        #region Text
        //
        // Summary:
        //     Gets or sets the text contents of a System.Windows.Controls.TextBlock.
        //
        // Returns:
        //     The text contents of this System.Windows.Controls.TextBlock. Note that all
        //     non-text content is stripped out, resulting in a plain text representation
        //     of the System.Windows.Controls.TextBlock contents. The default is System.String.Empty.
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RunningTextControl), new UIPropertyMetadata(string.Empty));
        #endregion

        #region Duration
        //
        // Summary:
        //     Gets or sets the length of time for which this timeline plays, not counting
        //     repetitions.
        //
        // Returns:
        //     The timeline's simple duration: the amount of time this timeline takes to
        //     complete a single forward iteration. The default value is System.Windows.Duration.Automatic.
        public int Duration
        {
            get { return (int)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(int), typeof(RunningTextControl), new UIPropertyMetadata(5));
        #endregion 
        #endregion
    }
}
