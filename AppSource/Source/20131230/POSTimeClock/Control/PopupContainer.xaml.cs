#region history

// Popup contains the control which you want to show popup.
//
// Name: Arron
// Date: 08/21/2012 original created
// 
// Add: 
//
// Modified: 

#endregion

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CPC.TimeClock;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for ScheduleManagement.xaml
    /// </summary>
    public partial class PopupContainer : Window
    {
        #region Properties

        public bool ShowMinimize
        {
            get
            {
                return systemButtons.ButtonMinimize;
            }
            set
            {
                systemButtons.ButtonMinimize = value;
            }
        }

        public bool ShowMaximize
        {
            get
            {
                return systemButtons.ButtonMaximize;
            }
            set
            {
                systemButtons.ButtonMaximize = value;
            }
        }

        public bool ShowClose
        {
            get
            {
                return systemButtons.ButtonClose;
            }
            set
            {
                systemButtons.ButtonClose = value;
            }
        }

        public enum BorderStyle
        {
            None,
            FixToolWindow,
            Sizable
        }

        //private BorderStyle _formBorderStyle = BorderStyle.Sizable;
        public BorderStyle FormBorderStyle
        {
            set
            {
                switch (value)
                {
                    case BorderStyle.None:
                        this.BrdTopBar.Visibility = Visibility.Collapsed;
                        this.brdConfiguration.BorderThickness = new Thickness(0);
                        break;
                    case BorderStyle.FixToolWindow:
                        this.BrdTopBar.Visibility = Visibility.Visible;
                        ShowMinimize = false;
                        ShowMaximize = false;
                        break;
                    case BorderStyle.Sizable:
                        this.BrdTopBar.Visibility = Visibility.Visible;
                        ShowMinimize = true;
                        ShowMaximize = true;
                        break;
                }
            }
        }

        /// <summary>
        /// CanDragAll Default False  => Only drag on Header
        ///CanDragAll : drag any where
        /// </summary>
        public bool CanDragAll { get; set; }
        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupContainer()
        {
            this.InitializeComponent();
            if (App.Current.MainWindow.IsLoaded)
            {
                // Check the main window show previously.
                this.Owner = App.Current.MainWindow;
                BrdTopBar.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(BrdTopBar_MouseLeftButtonDown);
            }
        }

        public PopupContainer(UserControl content, bool dragAll = false)
            : this()
        {
            CanDragAll = dragAll;
            // Set properties to window
            this.Width = content.Width;
            this.Height = content.Height + BrdTopBar.Height;
            this.grdContent.Children.Add(content);
            this.ShowInTaskbar = false;

            this.UpdateLayout();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // Default
            this.FormBorderStyle = BorderStyle.FixToolWindow;
        }

        #endregion

        #region Events

        private void BrdTopBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            // Conflict with ImageViewer Control.
            if (CanDragAll)
                this.DragMove();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        protected virtual void PopupDragMove()
        {
            this.DragMove();
        }

        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must represent a valid Member Expression");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression must represent a valid Property on the object");

            handler(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }
        #endregion
    }
}