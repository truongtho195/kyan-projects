using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for SystemButton.xaml
    /// </summary>
    public partial class SystemButton : UserControl
    {

        #region Fields

        private Window window;

        #endregion

        #region Properties

        /// <summary>
        /// Show / Hidden Minimize Button
        /// </summary>
        public bool ButtonMinimize
        {
            get
            {
                return this.btnMinimize.Visibility == Visibility.Visible;
            }
            set
            {
                this.btnMinimize.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Show / Hidden Maximize Button
        /// </summary>
        public bool ButtonMaximize
        {
            get
            {
                return this.btnMaximize.Visibility == Visibility.Visible;
            }
            set
            {
                this.btnMaximize.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Show / Hidden Close Button
        /// </summary>
        public bool ButtonClose
        {
            get
            {
                return this.btnClose.Visibility == Visibility.Visible;
            }
            set
            {
                this.btnClose.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Item Width
        /// </summary>
        public double ItemWidth
        {
            get { return this.btnClose.Width; }
            set
            {
                if (this.btnClose.Width != value)
                {
                    this.btnMinimize.Width = value;
                    this.btnMaximize.Width = value;
                    this.btnClose.Width = value;
                }
            }
        }

        /// <summary>
        /// Item Height
        /// </summary>
        public double ItemHeight
        {
            get { return this.btnClose.Height; }
            set
            {
                if (this.btnClose.Height != value)
                {
                    this.btnMinimize.Height = value;
                    this.btnMaximize.Height = value;
                    this.btnClose.Height = value;
                }
            }
        }

        public Brush ItemBackground
        {
            get { return this.btnMinimize.Background; }
            set
            {
                if (this.btnMinimize.Background != value)
                {
                    this.btnMinimize.Background = value;
                    this.btnMaximize.Background = value;
                    this.btnClose.Background = value;
                }
            }
        }

        public Brush ItemForeground
        {
            get { return this.btnMinimize.Foreground; }
            set
            {
                if (this.btnMinimize.Foreground != value)
                {
                    this.btnMinimize.Foreground = value;
                    this.btnMaximize.Foreground = value;
                    this.btnClose.Foreground = value;
                }
            }
        }

        #endregion

        #region Construtors

        public SystemButton()
        {
            this.InitializeComponent();

            this.Loaded += new RoutedEventHandler(delegate
            {
                if (this.window == null) this.window = FindParentWindow(this);
                if (this.window != null)
                {
                    switch (this.window.WindowState)
                    {
                        case WindowState.Normal:
                            this.btnMaximize.IsChecked = false;
                            break;
                        default:
                            this.btnMaximize.IsChecked = true;
                            break;
                    }

                    this.window.StateChanged += new EventHandler(delegate
                    {
                        if (this.window != null)
                        {
                            switch (this.window.WindowState)
                            {
                                case WindowState.Normal:
                                    this.btnMaximize.IsChecked = false;
                                    break;
                                default:
                                    this.btnMaximize.IsChecked = true;
                                    break;
                            }
                        }
                    });
                }
            });

            this.btnMinimize.Click += new RoutedEventHandler(delegate
            {
                if (this.window != null)
                    this.window.WindowState = WindowState.Minimized;
            });

            this.btnMaximize.Click += new RoutedEventHandler(delegate
                {
                    if (this.window != null)
                    {
                        switch (this.window.WindowState)
                        {
                            case WindowState.Normal:
                                this.window.WindowState = WindowState.Maximized;
                                break;
                            default:
                                this.window.WindowState = WindowState.Normal;
                                break;
                        }
                    }
                });

            this.btnClose.Click += new RoutedEventHandler(delegate
                {
                    if (this.window != null)
                    {
                        this.window.Close();
                    }
                });
        }

        #endregion

        #region Methods

        private static Window FindParentWindow(DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            // Check if this is the end of the tree       
            if (parent == null) return null;
            if (parent is Window)
                return parent as Window;
            return FindParentWindow(parent);
        }

        #endregion

    }
}