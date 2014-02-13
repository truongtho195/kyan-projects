using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace CPCToolkitExt
{
    public class PopupControl : ContentControl
    {

        #region Contrustor
        public PopupControl()
        {
            this.MouseEnter += new MouseEventHandler(PopupControl_MouseEnter);
            this.MouseLeave += new MouseEventHandler(PopupControl_MouseLeave);
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(PopupControl_IsVisibleChanged);
        }

        #endregion

        #region Fields
        private bool _isFocused = false;
        #endregion

        #region Event
        void PopupControl_MouseLeave(object sender, MouseEventArgs e)
        {
            ///Set focus for control
            this._isFocused = false;
        }
        void PopupControl_MouseEnter(object sender, MouseEventArgs e)
        {
            ///Set focus for control
            this._isFocused = true;
        }
        void PopupControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                this.IsOpen = false;
                RaiseEvent(new RoutedEventArgs(ClosedEvent));
            }
        }
        void ContentControl_GotMouseCapture(object sender, MouseEventArgs e)
        {
            this.Dispatcher.BeginInvoke(
                       DispatcherPriority.Input,
                       (ThreadStart)delegate
                       {
                           if (!_isFocused)
                           {
                               this.Visibility = Visibility.Collapsed;
                           }
                       });
        }
        void ContentControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(
                          DispatcherPriority.Input,
                          (ThreadStart)delegate
                          {
                              if (!_isFocused)
                              {
                                  this.Visibility = Visibility.Collapsed;
                              }
                          });
        }
        #endregion

        #region Properties

        #region IsOpen
        public bool IsOpen { get; set; }
        #endregion

        #endregion

        #region DependencyProperty

        #region public double MaxDropDownHeight
        /// <summary>
        /// Gets or sets the maximum height of the drop-down portion of the
        /// <see cref="T:System.Wi
        /// ndows.Controls.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The maximum height of the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// The default is <see cref="F:System.Double.PositiveInfinity" />.</value>
        /// <exception cref="T:System.ArgumentException">The specified value is less than 0.</exception>
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.MaxDropDownHeight" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.MaxDropDownHeight" />
        /// dependency property.</value>
        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(
                "MaxDropDownHeight",
                typeof(double),
                typeof(PopupControl),
                new PropertyMetadata(double.PositiveInfinity, OnMaxDropDownHeightPropertyChanged));

        private static void OnMaxDropDownHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PopupControl source = d as PopupControl;
            if (e.NewValue != null)
                source.MaxHeight = double.Parse(e.NewValue.ToString());
        }

        #endregion public double MaxDropDownHeight

        #region ClosedEvent
        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
           "Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupControl));
        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        #endregion

        #endregion

        #region Methods
        #region Open
        //Set status
        public void Open(Window main, Point point, bool value)
        {
            if (value == true)
            {
                //Stopwatch st = new Stopwatch();
                //st.Start();
                main.MouseLeftButtonDown += new MouseButtonEventHandler(ContentControl_MouseLeftButtonDown);
                main.GotMouseCapture += new MouseEventHandler(ContentControl_GotMouseCapture);
                main.MouseUp += new MouseButtonEventHandler(main_MouseUp);
                this.HorizontalAlignment = HorizontalAlignment.Left;
                this.VerticalAlignment = VerticalAlignment.Top;
                this.Margin = new Thickness(point.X, point.Y, 0, 0);
                this.Visibility = Visibility.Visible;
                this.IsOpen = true;
                // st.Stop();
                // Debug.Write(st.Elapsed + "\n");
            }
            else
            {
                //Remove the popup
                this.Visibility = Visibility.Collapsed;
            }

        }

        void main_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(
                       DispatcherPriority.Input,
                       (ThreadStart)delegate
                       {
                           if (!_isFocused)
                           {
                               this.Visibility = Visibility.Collapsed;
                           }
                       });
        }
        #endregion
        #endregion
    }
}
