using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using CPC.POS.Interfaces;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        #region Fields

        private string _filePath;

        private const int _widthAvailable = 70;
        private const int _heightAvailable = 25;

        private Size _newSize;

        #endregion

        #region Contructors

        public Dashboard()
        {
            InitializeComponent();

            this.demonstratorList.ItemsSource = FindDemonstrators();

            this.stackPanel1.Drop += new DragEventHandler(ColumnDrop);
            this.stackPanel2.Drop += new DragEventHandler(ColumnDrop);
            this.stackPanel3.Drop += new DragEventHandler(ColumnDrop);

            GetDashboardItems();

            this.SizeChanged += new SizeChangedEventHandler(DashboardSizeChanged);
        }

        #endregion

        #region Methods

        #region FindDemonstrators

        /// <summary>
        /// Finds the demonstrators registered by interface.
        /// </summary>
        private static List<IDemonstrator> FindDemonstrators()
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
                type.GetInterface("IDemonstrator", false) != null);

            return types.Select(type => Activator.CreateInstance(type) as IDemonstrator).OrderBy(d => d.Description).ToList();
        }

        #endregion

        #region GetDashboardItems

        /// <summary>
        /// Gets all DashboardItems from XML.
        /// </summary>
        private void GetDashboardItems()
        {
            List<IDemonstrator> demonstratorList = this.demonstratorList.ItemsSource as List<IDemonstrator>;
            if (demonstratorList == null || !demonstratorList.Any())
            {
                return;
            }

            _filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Dashboard.xml");
            if (!File.Exists(_filePath))
            {
                _filePath = null;
                return;
            }

            FileStream stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream == null)
            {
                return;
            }

            XDocument xDocument = XDocument.Load(stream);
            stream.Dispose();
            stream = null;
            if (xDocument == null || xDocument.Root == null)
            {
                return;
            }

            // Quet sach.
            stackPanel1.Children.Clear();
            stackPanel2.Children.Clear();
            stackPanel3.Children.Clear();

            // Lay danh sach StackPanel trong XML.
            IEnumerable<XElement> xStackPanels = xDocument.Root.Elements("StackPanel");
            if (xStackPanels.Any())
            {
                // Sap xep danh sach StackPanel trong XML.
                xStackPanels = xStackPanels.OrderBy(x => (int)x.Attribute("Index"));
                int stackPanelIndex = 0;
                IEnumerable<XElement> xDemonstrators;
                foreach (XElement xStackPanel in xStackPanels)
                {
                    // Lay danh sach Demonstrator thuoc StackPanel trong XML.
                    xDemonstrators = xStackPanel.Elements("Demonstrator");
                    if (xDemonstrators.Any())
                    {
                        // Sap xep danh sach Demonstrator thuoc StackPanel trong XML.
                        xDemonstrators = xDemonstrators.OrderBy(x => (int)x.Attribute("Index"));
                        IDemonstrator demonstrator;
                        DashboardItem dashboardItem;
                        XElement xConfiguration;
                        foreach (XElement xDemonstrator in xDemonstrators)
                        {
                            // Lay doi tuong Demonstrator ung voi Demonstrator trong XML.
                            demonstrator = demonstratorList.FirstOrDefault(x => x.Name == xDemonstrator.Attribute("Name").Value);
                            xConfiguration = xDemonstrator.Element("Configuration");
                            if (demonstrator != null)
                            {
                                dashboardItem = new DashboardItem();
                                dashboardItem.Child = demonstrator.Create(xConfiguration);
                                dashboardItem.DemonstratorName = demonstrator.Name;
                                dashboardItem.Title = demonstrator.Title;
                                dashboardItem.Margin = new Thickness(0, 2, 0, 2);
                                dashboardItem.Lock();
                                if (stackPanelIndex == 0)
                                {
                                    dashboardItem.ParentStackPanel = stackPanel1;
                                    stackPanel1.Children.Add(dashboardItem);
                                }
                                else if (stackPanelIndex == 1)
                                {
                                    dashboardItem.ParentStackPanel = stackPanel2;
                                    stackPanel2.Children.Add(dashboardItem);
                                }
                                else
                                {
                                    dashboardItem.ParentStackPanel = stackPanel3;
                                    stackPanel3.Children.Add(dashboardItem);
                                }
                            }
                        }
                    }

                    stackPanelIndex++;
                }
            }
        }

        #endregion

        #region SaveDashboardItems

        /// <summary>
        /// Save DashboardItems.
        /// </summary>
        private bool SaveDashboardItems()
        {
            bool isSuccess = true;

            try
            {
                if (string.IsNullOrWhiteSpace(_filePath))
                {
                    throw new Exception("File not found!");
                }

                XElement xStackPanel1 = new XElement("StackPanel",
                             new XAttribute("Key", "StackPanel1"),
                             new XAttribute("Index", 0));
                XElement xStackPanel2 = new XElement("StackPanel",
                           new XAttribute("Key", "StackPanel2"),
                           new XAttribute("Index", 1));
                XElement xStackPanel3 = new XElement("StackPanel",
                           new XAttribute("Key", "StackPanel3"),
                           new XAttribute("Index", 2));
                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Dashboard",
                        xStackPanel1,
                        xStackPanel2,
                        xStackPanel3));

                XElement xConfiguration;
                foreach (DashboardItem dashboardItem in stackPanel1.Children)
                {
                    xConfiguration = (dashboardItem.Child.DataContext as IDashboardItemFunction).GetConfiguration();
                    if (xConfiguration != null)
                    {
                        xStackPanel1.Add(new XElement("Demonstrator",
                            new XAttribute("Name", dashboardItem.DemonstratorName),
                            new XAttribute("Index", stackPanel1.Children.IndexOf(dashboardItem)),
                            new XElement(xConfiguration)));
                    }
                    else
                    {
                        xStackPanel1.Add(new XElement("Demonstrator",
                            new XAttribute("Name", dashboardItem.DemonstratorName),
                            new XAttribute("Index", stackPanel1.Children.IndexOf(dashboardItem))));
                    }
                }

                foreach (DashboardItem dashboardItem in stackPanel2.Children)
                {
                    xConfiguration = (dashboardItem.Child.DataContext as IDashboardItemFunction).GetConfiguration();
                    if (xConfiguration != null)
                    {
                        xStackPanel2.Add(new XElement("Demonstrator",
                            new XAttribute("Name", dashboardItem.DemonstratorName),
                            new XAttribute("Index", stackPanel2.Children.IndexOf(dashboardItem)),
                            new XElement(xConfiguration)));
                    }
                    else
                    {
                        xStackPanel2.Add(new XElement("Demonstrator",
                            new XAttribute("Name", dashboardItem.DemonstratorName),
                            new XAttribute("Index", stackPanel2.Children.IndexOf(dashboardItem))));
                    }
                }

                foreach (DashboardItem dashboardItem in stackPanel3.Children)
                {
                    xConfiguration = (dashboardItem.Child.DataContext as IDashboardItemFunction).GetConfiguration();
                    if (xConfiguration != null)
                    {
                        xStackPanel3.Add(new XElement("Demonstrator",
                            new XAttribute("Name", dashboardItem.DemonstratorName),
                            new XAttribute("Index", stackPanel3.Children.IndexOf(dashboardItem)),
                            new XElement(xConfiguration)));
                    }
                    else
                    {
                        xStackPanel3.Add(new XElement("Demonstrator",
                            new XAttribute("Name", dashboardItem.DemonstratorName),
                            new XAttribute("Index", stackPanel3.Children.IndexOf(dashboardItem))));
                    }
                }

                doc.Save(_filePath);
            }
            catch (Exception exception)
            {
                isSuccess = false;
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return isSuccess;
        }

        #endregion

        #region Lock

        /// <summary>
        /// Lock all DashboardItem.
        /// </summary>
        private void Lock()
        {
            Lock(stackPanel1);
            Lock(stackPanel2);
            Lock(stackPanel3);
        }

        /// <summary>
        /// Lock all DashboardItem in StackPanel.
        /// </summary>
        private void Lock(StackPanel stackPanel)
        {
            stackPanel.AllowDrop = false;
            if (stackPanel.Children.Count > 0)
            {
                foreach (var item in stackPanel.Children)
                {
                    (item as DashboardItem).Lock();
                }
            }
        }

        #endregion

        #region Unlock

        /// <summary>
        /// Unlock all DashboardItem.
        /// </summary>
        private void Unlock()
        {
            Unlock(stackPanel1);
            Unlock(stackPanel2);
            Unlock(stackPanel3);
        }

        /// <summary>
        /// Unlock all DashboardItem in StackPanel.
        /// </summary>
        private void Unlock(StackPanel stackPanel)
        {
            stackPanel.AllowDrop = true;
            if (stackPanel.Children.Count > 0)
            {
                foreach (var item in stackPanel.Children)
                {
                    (item as DashboardItem).Unlock();
                }
            }
        }

        #endregion

        #endregion

        #region Events

        #region ColumnDrop

        private void ColumnDrop(object sender, DragEventArgs e)
        {
            DashboardItem sourceItem = e.Data.GetData(typeof(DashboardItem)) as DashboardItem;
            if (sourceItem == null)
            {
                return;
            }

            if (!sourceItem.IsDropped)
            {
                StackPanel stackPanel = sender as StackPanel;
                if (sourceItem.Parent != null)
                {
                    int sourceItemIndex = sourceItem.ParentStackPanel.Children.IndexOf(sourceItem);
                    sourceItem.ParentStackPanel.Children.RemoveAt(sourceItemIndex);
                    stackPanel.Children.Add(sourceItem);
                    sourceItem.ParentStackPanel = stackPanel;
                }
                else
                {
                    stackPanel.Children.Add(sourceItem);
                    sourceItem.ParentStackPanel = stackPanel;
                }
            }
            else
            {
                sourceItem.IsDropped = false;
            }

        }

        #endregion

        #region DemonstratorMouseDoubleClick

        private void DemonstratorMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IDemonstrator demonstrator = (sender as ListBoxItem).Content as IDemonstrator;
            DashboardItem dashboardItem = new DashboardItem();
            dashboardItem.Child = demonstrator.Create();
            dashboardItem.DemonstratorName = demonstrator.Name;
            dashboardItem.Title = demonstrator.Title;
            dashboardItem.Margin = new Thickness(0, 2, 0, 2);
            dashboardItem.ParentStackPanel = stackPanel1;
            dashboardItem.Unlock();
            stackPanel1.Children.Add(dashboardItem);

            if (_newSize != null)
            {
                IDashboardItemFunction dashboardItemFunction = dashboardItem.Child.DataContext as IDashboardItemFunction;
                if (dashboardItemFunction != null)
                {
                    dashboardItemFunction.UpdateSize(_newSize);
                }
            }
        }

        #endregion

        #region DemonstratorMouseMove

        private void DemonstratorMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IDemonstrator demonstrator = (sender as ListBoxItem).Content as IDemonstrator;
                DashboardItem draggedItem = new DashboardItem();
                draggedItem.Child = demonstrator.Create();
                draggedItem.DemonstratorName = demonstrator.Name;
                draggedItem.Title = demonstrator.Title;
                draggedItem.Margin = new Thickness(0, 2, 0, 2);
                draggedItem.Unlock();
                if (_newSize != null)
                {
                    IDashboardItemFunction dashboardItemFunction = draggedItem.Child.DataContext as IDashboardItemFunction;
                    if (dashboardItemFunction != null)
                    {
                        dashboardItemFunction.UpdateSize(_newSize);
                    }
                }
                DragDrop.DoDragDrop(draggedItem, draggedItem, DragDropEffects.Move);
            }
        }

        #endregion

        #region ButtonChangeClick

        private void ButtonChangeClick(object sender, RoutedEventArgs e)
        {
            buttonChange.Visibility = System.Windows.Visibility.Collapsed;
            buttonBack.Visibility = System.Windows.Visibility.Visible;
            gridLeft.Visibility = System.Windows.Visibility.Visible;
            rectangle1.Visibility = System.Windows.Visibility.Visible;
            rectangle2.Visibility = System.Windows.Visibility.Visible;
            columnLeft.Width = new GridLength(250);
            Unlock();
        }

        #endregion

        #region ButtonBackClick

        private void ButtonBackClick(object sender, RoutedEventArgs e)
        {
            buttonChange.Visibility = System.Windows.Visibility.Visible;
            buttonBack.Visibility = System.Windows.Visibility.Collapsed;
            gridLeft.Visibility = System.Windows.Visibility.Collapsed;
            rectangle1.Visibility = System.Windows.Visibility.Collapsed;
            rectangle2.Visibility = System.Windows.Visibility.Collapsed;
            columnLeft.Width = GridLength.Auto;
            Lock();
            SaveDashboardItems();
        }

        #endregion

        #region DashboardSizeChanged

        private void DashboardSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 0)
            {
                IDashboardItemFunction dashboardItemFunction;
                _newSize = e.NewSize;
                _newSize.Width -= _widthAvailable;
                _newSize.Height -= _heightAvailable;

                if (stackPanel1.Children.Count > 0)
                {
                    foreach (DashboardItem dashboardItem in stackPanel1.Children)
                    {
                        dashboardItemFunction = dashboardItem.Child.DataContext as IDashboardItemFunction;
                        if (dashboardItemFunction != null)
                        {
                            dashboardItemFunction.UpdateSize(_newSize);
                        }
                    }
                }

                if (stackPanel2.Children.Count > 0)
                {
                    foreach (DashboardItem dashboardItem in stackPanel2.Children)
                    {
                        dashboardItemFunction = dashboardItem.Child.DataContext as IDashboardItemFunction;
                        if (dashboardItemFunction != null)
                        {
                            dashboardItemFunction.UpdateSize(_newSize);
                        }
                    }
                }

                if (stackPanel3.Children.Count > 0)
                {
                    foreach (DashboardItem dashboardItem in stackPanel3.Children)
                    {
                        dashboardItemFunction = dashboardItem.Child.DataContext as IDashboardItemFunction;
                        if (dashboardItemFunction != null)
                        {
                            dashboardItemFunction.UpdateSize(_newSize);
                        }
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
