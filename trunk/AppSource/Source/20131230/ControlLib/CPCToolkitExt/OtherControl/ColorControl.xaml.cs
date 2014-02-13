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
using System.Windows.Threading;
using System.Threading;

namespace CPCToolkitExt
{
    /// <summary>
    /// Interaction logic for ColorControl.xaml
    /// </summary>
    public partial class ColorControl : UserControl
    {
        #region Constructor
        public ColorControl()
        {
            InitializeComponent();
            this.listboxColor.ItemsSource = typeof(Brushes).GetProperties();
            this.btnShowDataGrid.Click += new RoutedEventHandler(ShowColor_Click);
            this.Loaded += new RoutedEventHandler(ColorControl_Loaded);
        } 
        #endregion

        #region The event of Control.

        private void ColorControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Set ColorControl type.
            if (ControlType == ControlType.TextColor)
                this.ToolTip = "Set text color.";
            else
                this.ToolTip = "Set text highlight color.";

        }
        private void ColorItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                String colorName = (sender as ListBoxItem).Tag.ToString();
                //SolidColorBrush color = ((sender as ListBoxItem).Content) as SolidColorBrush;
                SolidColorBrush brush = new BrushConverter().ConvertFromString(colorName) as SolidColorBrush;
                this.SelectedBrush = brush;//(sender as ListBoxItem).Content.GetType().Name as SolidColorBrush;
                this.ClosePopup();
                //Raises a SelectColorChanged event.
                this.Dispatcher.BeginInvoke(
                           DispatcherPriority.Input,
                           (ThreadStart)delegate
                           {
                               this.RaiseEvent(new RoutedEventArgs(SelectColorChangedEvent));
                           });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void ShowColor_Click(object sender, RoutedEventArgs e)
        {
            this.OpenPopup();
        } 
        #endregion

        #region DependencyProperties
        
        #region SelectedBrush
        public Brush SelectedBrush
        {
            get { return (Brush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for SelectedBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(ColorControl), new UIPropertyMetadata(Brushes.Black)); 
        #endregion
        
        #region ControlType
        public ControlType ControlType
        {
            get { return (ControlType)GetValue(ControlTypeProperty); }
            set { SetValue(ControlTypeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ControlType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ControlTypeProperty =
            DependencyProperty.Register("ControlType", typeof(ControlType), typeof(ColorControl), new UIPropertyMetadata(ControlType.TextColor)); 
        #endregion

        #region ClosedEvent
        public static readonly RoutedEvent SelectColorChangedEvent = EventManager.RegisterRoutedEvent(
           "SelectColorChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorControl));
        public event RoutedEventHandler SelectColorChanged
        {
            add { AddHandler(SelectColorChangedEvent, value); }
            remove { RemoveHandler(SelectColorChangedEvent, value); }
        } 
        #endregion

        #endregion

        #region Methods
         private void OpenPopup()
        {
            this.popupColor.StaysOpen = false;
            this.popupColor.IsOpen = true;
        }
        private void ClosePopup()
        {
            this.popupColor.StaysOpen = true;
            this.popupColor.IsOpen = false;
        }
        #endregion
    }

    public enum ControlType { TextColor = 0, HighlightColor = 1 }
}
