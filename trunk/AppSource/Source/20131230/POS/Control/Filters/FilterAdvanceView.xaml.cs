using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xaml;
using CustomTextBox;
using CPC.POS;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class FilterAdvanceView : UserControl
    {

        #region define
        readonly ButtonItem _buttonAdd = new ButtonItem
        {
            Key = ButtonType.Add,
            Content = "Add",
            Height = 23
        };

        readonly ButtonItem _buttonRemove = new ButtonItem
        {
            Key = ButtonType.Remove,
            Content = "Remove",
            Height = 23
        };
        #endregion

        #region Dependency Properties

        #region ItemsSource

        public ObservableCollection<object> ItemsSource
        {
            get
            {
                return (ObservableCollection<object>)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for SearchCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<object>), typeof(FilterAdvanceView),
            new FrameworkPropertyMetadata(null,
                new PropertyChangedCallback(FilterAdvanceView.OnItemsSourceChanged)));

        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as FilterAdvanceView).DrawItemsSource(e.NewValue as ObservableCollection<object>);
        }

        #endregion

        #region Result Collection

        public ObservableCollection<FilterItemModel> SelectedItems
        {
            get { return (ObservableCollection<FilterItemModel>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<FilterItemModel>), typeof(FilterAdvanceView));

        #endregion

        #region Add Button

        public string AddButtonName
        {
            get { return (string)GetValue(AddButtonNameProperty); }
            set { SetValue(AddButtonNameProperty, value); }
        }

        public static readonly DependencyProperty AddButtonNameProperty =
            DependencyProperty.Register("AddButtonName", typeof(string), typeof(FilterAdvanceView), new UIPropertyMetadata("Add"));

        #endregion

        #region Remove Button Name

        public string RemoveButtonName
        {
            get { return (string)GetValue(RemoveButtonNameProperty); }
            set { SetValue(RemoveButtonNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RemoveButtonNameProperty =
            DependencyProperty.Register("RemoveButtonName", typeof(string), typeof(FilterAdvanceView), new UIPropertyMetadata("Remove"));

        #endregion

        #endregion

        #region Command

        // Command

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(FilterAdvanceView), new UIPropertyMetadata(null));

        #endregion

        #region Fields

        private IList<FrameworkElement> _itemsCollection = new List<FrameworkElement>();
        private IList<FilterComboBox> _listItems;
        private ComboBox _selectedComboBox = null;
        private int _index = 1;

        #endregion

        #region Properties

        private int _maxRows = 5;
        public int MaxRows
        {
            get { return _maxRows; }
            set
            {
                if (_maxRows != value)
                {
                    _maxRows = value;
                }
            }
        }

        #endregion

        #region Constructors

        public FilterAdvanceView()
        {
            InitializeComponent();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            UpdateResult();

            base.OnLostFocus(e);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update Result Method
        /// </summary>
        private void UpdateResult()
        {
            if (null == SelectedItems) return;

            SelectedItems.Clear();

            if (null == _listItems) return;

            foreach (var item in _listItems)
            {
                SelectedItems.Add(item.SelectedItem);
            }
        }

        public void ClearData()
        {
            if (null == _listItems) return;

            foreach (var item in _listItems)
            {
                if (null == item.SelectedItem) continue;

                item.SelectedItem.Value1 = null;
                item.SelectedItem.Value2 = null;

                if (item.SelectedItem.ItemsSource is IList<StatusModel>)
                {
                    foreach (StatusModel status in item.SelectedItem.ItemsSource as IList<StatusModel>)
                    {
                        status.IsChecked = false;
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add Search Item
        /// </summary>
        /// <param name="model"></param>
        private void AddItem(FilterItemModel model)
        {
            // Create the filter column in left
            FilterComboBox comboBox = new FilterComboBox
            {
                ItemContainerStyleSelector = new SeparatorStyleSelector(),
                Background = null,
                VerticalAlignment = VerticalAlignment.Center
            };
            comboBox.Name = String.Concat("COMBO", _index++);
            Binding binding = new Binding { Path = new PropertyPath("ItemsSource"), ElementName = "filterView" };
            comboBox.FilterSelection = new DelegateFilterSelection(FilterSelection);
            comboBox.SetBinding(FilterComboBox.ItemsSourceProperty, binding);
            comboBox.SelectedValuePath = "ValueMember";
            comboBox.DisplayMemberPath = "DisplayMember";
            comboBox.SetValue(FilterComboBox.SelectedItemProperty, model);
            comboBox.SetValue(Grid.ColumnProperty, 0);
            comboBox.SetValue(Grid.RowProperty, this.gridSearch.RowDefinitions.Count);
            comboBox.DropDownOpened += new EventHandler(comboBox_DropDownOpened);
            comboBox.KeywordElement = GetKeywordElement(comboBox); // model, this.gridSearch.RowDefinitions.Count);

            // Manage the control list
            if (null != _listItems)
            {
                _listItems.Add(comboBox);
            }

            this.gridSearch.RowDefinitions.Add(new RowDefinition { Height = new GridLength(26) });
            this.gridSearch.RegisterName(comboBox.Name, comboBox);
            this.gridSearch.Children.Add(comboBox);
            this.gridSearch.Children.Add(comboBox.KeywordElement);
        }

        private FrameworkElement FilterSelection(FilterComboBox combo)
        {
            if (combo.KeywordElement != null)
            {
                this.gridSearch.Children.Remove(combo.KeywordElement);
            }

            FrameworkElement element = GetKeywordElement(combo);
            this.gridSearch.Children.Add(element);
            return element;
        }

        private FrameworkElement GetKeywordElement(FilterComboBox combo) // FilterItemModel model, int rowIndex)
        {
            FrameworkElement element = null;

            if (null == combo.SelectedItem)
            {
                TextBox textbox = new TextBox
                {
                    Height = 26,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Binding binding = new Binding { Path = new PropertyPath("SelectedItem.Value1"), Source = combo, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                textbox.SetBinding(TextBox.TextProperty, binding);
                element = textbox;
            }
            else
            {
                switch (combo.SelectedItem.Type)
                {
                    case SearchType.Numeric | SearchType.Currency:
                        TextBoxNumber textBoxNumber = new TextBoxNumber
                        {
                            Height = 26,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Binding binding = new Binding { Path = new PropertyPath("SelectedItem.Value1"), Source = combo, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                        textBoxNumber.SetBinding(TextBoxNumber.TextRealDependencyProperty, binding);
                        element = textBoxNumber;
                        break;
                    case SearchType.Status:
                        StatusComboBox statusComboBox = new StatusComboBox
                        {
                            Height = 26,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        binding = new Binding { Path = new PropertyPath("SelectedItem.Value1"), Source = combo, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                        statusComboBox.SetBinding(StatusComboBox.SelectedItemsProperty, binding);
                        binding = new Binding { Path = new PropertyPath("SelectedItem.ItemsSource"), Source = combo, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                        statusComboBox.SetBinding(StatusComboBox.ItemsSourceProperty, binding);

                        if (combo.SelectedItem.ItemsSource != null)
                        {
                            foreach (StatusModel item in combo.SelectedItem.ItemsSource as IList<StatusModel>)
                            {
                                item.IsChecked = false;
                            }
                        }

                        element = statusComboBox;
                        break;
                    case SearchType.Date:
                        DateComboBox dateComboBox = new DateComboBox
                        {
                            Height = 26,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        binding = new Binding { Path = new PropertyPath("SelectedItem.Value1"), Source = combo, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                        dateComboBox.SetBinding(DateComboBox.FromDateProperty, binding);
                        binding = new Binding { Path = new PropertyPath("SelectedItem.Value2"), Source = combo, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                        dateComboBox.SetBinding(DateComboBox.ToDateProperty, binding);
                        element = dateComboBox;
                        break;
                    default: // Text
                        TextBox textbox = new TextBox
                        {
                            Height = 26,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        binding = new Binding { Path = new PropertyPath("SelectedItem.Value1"), Source = combo, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                        textbox.SetBinding(TextBox.TextProperty, binding);
                        element = textbox;
                        break;
                }
            }

            element.SetValue(MarginProperty, new Thickness(0, 3, 0, 3));
            element.SetValue(Grid.ColumnProperty, 1);
            element.SetValue(Grid.RowProperty, combo.GetValue(Grid.RowProperty));
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                element.Focus();
            }));

            return element;
        }

        private void comboBox_DropDownOpened(object sender, EventArgs e)
        {
            _selectedComboBox = sender as ComboBox;
        }

        /// <summary>
        /// IsNavigationKey Method
        /// </summary>
        /// <param name="Pressed"></param>
        /// <returns></returns>
        private bool IsNavigationKey(Key pressed)
        {
            return pressed == Key.Up
                || pressed == Key.Down
                || pressed == Key.Left
                || pressed == Key.Right
                || pressed == Key.NumPad2
                || pressed == Key.NumPad4
                || pressed == Key.NumPad6
                || pressed == Key.NumPad8
                || pressed == Key.PageUp
                || pressed == Key.PageDown
                || pressed == Key.Home
                || pressed == Key.End;
        }

        #endregion

        #region Events

        /// <summary>
        /// Key Down
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Filter();
            }

            base.OnKeyDown(e);
        }

        public void Filter()
        {
            if (null != Command)
            {
                // Ex: Execute the search command
                UpdateResult();

                // Excute command search on viewmodel.
                Command.Execute(SelectedItems);
            }
        }

        public void FilterText()
        {
            if (null != Command)
            {
                // Ex: Execute the search command
                UpdateResult();

                // Excute command search on viewmodel.
                Command.Execute(null);
            }
        }

        private void DrawItemsSource(ObservableCollection<object> collection)
        {
            // Init the variables
            SelectedItems = new ObservableCollection<FilterItemModel>();

            _listItems = new List<FilterComboBox>();

            // Add the default items into grid
            int countItems = 0;
            foreach (var item in collection.Where(s => s is FilterItemModel && (s as FilterItemModel).IsDefault))
            {
                if (countItems > _maxRows)
                    break;

                this.AddItem(item as FilterItemModel);

                // Increase
                countItems++;
            }

            int rowsCount = this.gridSearch.RowDefinitions.Count;
            int length = collection.Count(s => s is FilterItemModel);

            if (rowsCount < length)
            {
                collection.Add(_buttonAdd);
            }

            if (rowsCount > 1)
            {
                collection.Add(_buttonRemove);
            }

            SetValue(ItemsSourceProperty, collection);
        }

        /// <summary>
        /// Click Add / Remove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonItem button = sender as ButtonItem;

            // Close combo box
            _selectedComboBox.IsDropDownOpen = false;

            switch (button.Key)
            {
                case ButtonType.Add:

                    AddItem(null); // Add the empty item.

                    int rowsCount = this.gridSearch.RowDefinitions.Count;
                    int length = ItemsSource.Count(s => s is FilterItemModel);

                    if (rowsCount > 1 && ItemsSource.Count(x => x is ButtonItem && (x as ButtonItem).Key == ButtonType.Remove) == 0)
                    {
                        ItemsSource.Add(_buttonRemove);
                    }

                    if (rowsCount >= Math.Min(length, _maxRows))
                    {
                        // Remove the add button when enough source or max rows.
                        ItemsSource.Remove(_buttonAdd);
                    }

                    break;
                case ButtonType.Remove:
                    // Get index, ever available.
                    int index = Convert.ToInt32(_selectedComboBox.GetValue(Grid.RowProperty));

                    length = _listItems.Count;

                    // Clear source of the selected combobox item
                    FilterComboBox comboBox = _listItems[index];
                    //comboBox.ItemsSource = null;

                    if (null != comboBox.SelectedItem)
                    {
                        // Inactive it
                        comboBox.SelectedItem.IsSelected = false;
                    }

                    // Update rank of the item
                    for (int i = index + 1; i < length; i++)
                    {
                        var comboBox1 = _listItems.Single(x => (int)x.GetValue(Grid.RowProperty) == i);
                        comboBox1.SetValue(Grid.RowProperty, i - 1);
                        comboBox1.KeywordElement.SetValue(Grid.RowProperty, i - 1);
                    }

                    // Remove controls
                    this.gridSearch.Children.Remove(comboBox);
                    this.gridSearch.Children.Remove(comboBox.KeywordElement);
                    this.gridSearch.RowDefinitions.RemoveAt(index);

                    // Remove item
                    _listItems.RemoveAt(index);

                    if (_listItems.Count == 1)
                    {
                        ItemsSource.Remove(_buttonRemove);
                    }

                    // Insert the add button

                    rowsCount = this.gridSearch.RowDefinitions.Count;
                    length = ItemsSource.Count(s => s is FilterItemModel);

                    if (rowsCount < Math.Min(length, _maxRows) && ItemsSource.Count(x => x is ButtonItem && (x as ButtonItem).Key == ButtonType.Add) == 0)
                    {
                        if (ItemsSource.LastOrDefault() is ButtonItem)
                            ItemsSource.Insert(ItemsSource.Count - 1, _buttonAdd);
                        else
                            ItemsSource.Add(_buttonAdd);
                    }

                    break;
            }
        }

        #region DeepClone

        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        #endregion

        #endregion

    }

    #region Support Classes
    public class SeparatorData
    {
    }

    public class SeparatorStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is SeparatorData)
            {
                return (Style)((FrameworkElement)container).FindResource("separatorStyle");
            }
            else if (item is Button)
            {
                return (Style)((FrameworkElement)container).FindResource("buttonStyle");
            }
            else if (item is FilterItemModel)
            {
                return (Style)((FrameworkElement)container).FindResource("itemStyle");
            }
            return null;
        }
    }
    #endregion

}