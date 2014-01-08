using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CPC.POS.Interfaces;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for DashboardItem.xaml
    /// </summary>
    public partial class DashboardItem : UserControl
    {
        #region Contructors

        public DashboardItem()
        {
            InitializeComponent();
            this.Drop += new DragEventHandler(DashboardItemrDrop);
            this.buttonClose.Click += new RoutedEventHandler(ButtonCloseClick);
            this.buttonEdit.Click += new RoutedEventHandler(ButtonEditClick);
        }

        #endregion

        #region Properties

        #region Title

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DashboardItem), new UIPropertyMetadata(null));

        #endregion

        #region Child

        /// <summary>
        /// Gets or sets UserControl that object holds.
        /// </summary>
        public UserControl Child
        {
            get
            {
                return (UserControl)GetValue(ChildProperty);
            }
            set
            {
                SetValue(ChildProperty, value);
            }
        }

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register("Child", typeof(UserControl), typeof(DashboardItem), new UIPropertyMetadata(null, new PropertyChangedCallback(ChildPropertyChanged)));


        private static void ChildPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            DashboardItem sender = s as DashboardItem;
            if (sender.controlHolder != null)
            {
                sender.controlHolder.Children.Clear();
                sender.controlHolder.Children.Add((UserControl)e.NewValue);
            }
        }

        #endregion

        #region ParentStackPanel

        /// <summary>
        /// Gets or sets parent StackPanel that contains this object.
        /// </summary>
        public StackPanel ParentStackPanel
        {
            get
            {
                return (StackPanel)GetValue(ParentStackPanelProperty);
            }
            set
            {
                SetValue(ParentStackPanelProperty, value);
            }
        }

        public static readonly DependencyProperty ParentStackPanelProperty =
            DependencyProperty.Register("ParentStackPanel", typeof(StackPanel), typeof(DashboardItem), new UIPropertyMetadata(null));


        #endregion

        #region IsDropped

        /// <summary>
        /// Determine whether this object have been dropped.
        /// </summary>
        public bool IsDropped
        {
            get;
            set;
        }

        #endregion

        #region DemonstratorName

        /// <summary>
        /// Gets or sets name identity.
        /// </summary>
        public string DemonstratorName
        {
            get;
            set;
        }

        #endregion

        #endregion

        #region Methods

        #region Lock

        /// <summary>
        /// Lock DashboardItem.
        /// </summary>
        public void Lock()
        {
            this.buttonClose.Visibility = Visibility.Collapsed;
            this.buttonEdit.Visibility = Visibility.Collapsed;
            this.titleBar.Visibility = Visibility.Collapsed;
            this.titleBar.MouseMove -= TitleBarMouseMove;
            IDashboardItemFunction dashboardItemFunction = this.Child.DataContext as IDashboardItemFunction;
            if (dashboardItemFunction != null)
            {
                dashboardItemFunction.Lock();
            }
        }

        #endregion

        #region Unlock

        /// <summary>
        /// Unlock DashboardItem.
        /// </summary>
        public void Unlock()
        {
            IDashboardItemFunction dashboardItemFunction = this.Child.DataContext as IDashboardItemFunction;
            this.buttonClose.Visibility = Visibility.Visible;
            if (dashboardItemFunction != null && dashboardItemFunction.CanEdit)
            {
                this.buttonEdit.Visibility = Visibility.Visible;
            }
            else
            {
                this.buttonEdit.Visibility = Visibility.Collapsed;
            }
            this.titleBar.Visibility = Visibility.Visible;
            this.titleBar.MouseMove += TitleBarMouseMove;
        }

        #endregion

        #endregion

        #region Events

        #region TitleBarMouseMove

        private void TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }
        }

        #endregion

        #region DashboardItemrDrop

        private void DashboardItemrDrop(object sender, DragEventArgs e)
        {
            DashboardItem targetItem = sender as DashboardItem;
            DashboardItem sourceItem = e.Data.GetData(typeof(DashboardItem)) as DashboardItem;
            if (sourceItem == null)
            {
                return;
            }

            int sourceItemIndex = 0;
            int targetItemIndex = 0;
            if (sourceItem.Parent != null)
            {
                sourceItemIndex = sourceItem.ParentStackPanel.Children.IndexOf(sourceItem);
            }
            else
            {
                sourceItemIndex = targetItem.ParentStackPanel.Children.Count;
            }
            targetItemIndex = targetItem.ParentStackPanel.Children.IndexOf(targetItem);

            if (sourceItemIndex < targetItemIndex)
            {
                sourceItem.ParentStackPanel.Children.RemoveAt(sourceItemIndex);
                targetItem.ParentStackPanel.Children.Insert(targetItemIndex, sourceItem);
            }
            else
            {
                if (sourceItem.Parent != null)
                {
                    sourceItem.ParentStackPanel.Children.RemoveAt(sourceItemIndex);
                }
                targetItem.ParentStackPanel.Children.Insert(targetItemIndex, sourceItem);
            }

            sourceItem.ParentStackPanel = targetItem.ParentStackPanel;
            sourceItem.IsDropped = true;
        }

        #endregion

        #region ButtonCloseClick

        private void ButtonCloseClick(object sender, RoutedEventArgs e)
        {
            this.ParentStackPanel.Children.Remove(this);
        }

        #endregion

        #region ButtonEditClick

        private void ButtonEditClick(object sender, RoutedEventArgs e)
        {
            IDashboardItemFunction dashboardItemFunction = this.Child.DataContext as IDashboardItemFunction;
            if (dashboardItemFunction != null && dashboardItemFunction.CanEdit)
            {
                dashboardItemFunction.Unlock();
            }
        }

        #endregion

        #endregion
    }
}
