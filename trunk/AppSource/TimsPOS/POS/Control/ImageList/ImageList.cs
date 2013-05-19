using System;
using System.Linq;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;

namespace CPC.Control
{
    [TemplatePart(Name = "PART_ListBox", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]
    [TemplatePart(Name = "PART_ButtonAdd", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonChange", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonRemove", Type = typeof(Button))]
    [TemplatePart(Name = "PART_GRIDBUTTON", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_GRIDLISTBOX", Type = typeof(Grid))]
    public class ImageList : ListBox
    {
        #region Fields

        /// <summary>
        /// Reference PART_ListBox.
        /// </summary>
        private ListBox _listBox = null;

        /// <summary>
        /// Reference PART_Image.
        /// </summary>
        private Image _image = null;

        /// <summary>
        /// Reference PART_ButtonAdd.
        /// </summary>
        private Button _buttonAdd = null;

        /// <summary>
        /// Reference PART_ButtonChange.
        /// </summary>
        private Button _buttonChange = null;

        /// <summary>
        /// Reference PART_ButtonRemove.
        /// </summary>
        private Button _buttonRemove = null;

        /// <summary>
        /// Dot character.
        /// </summary>
        private readonly char _dot = '.';

        /// <summary>
        /// Keep DisplayMemberPath final.
        /// </summary>
        private string _displayMemberPath;

        /// <summary>
        ///To Reference selected object which contains image.
        /// </summary>
        private object _currentItem = null;

        /// <summary>
        /// To get grid which contain controls.
        /// </summary>
        private Grid _gridButton = null;

        /// <summary>
        /// To get grid which show image.
        /// </summary>
        private Grid _gridImage = null;

        /// <summary>
        /// To get grid which contain image list.
        /// </summary>
        private Grid _gridContentImage = null;
        #endregion

        #region Contructor

        static ImageList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageList), new FrameworkPropertyMetadata(typeof(ImageList)));
        }
        #endregion

        #region Properties

        #region IsReadOnly

        /// <summary>
        /// Gets or sets a value that indicates whether the user can add, delete or change images in the ImageList.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return (bool)GetValue(IsReadOnlyProperty);
            }
            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ImageList), new UIPropertyMetadata(false, IsReadOnlyChanged));

        public static void IsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageList imageList = d as ImageList;

            if ((bool)e.NewValue)
            {
                // Collapsed add, change, remove button.
                if (imageList._buttonAdd != null)
                {
                    imageList._buttonAdd.Visibility = Visibility.Collapsed;
                }
                if (imageList._buttonChange != null)
                {
                    imageList._buttonChange.Visibility = Visibility.Collapsed;
                }
                if (imageList._buttonRemove != null)
                {
                    imageList._buttonRemove.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Show add, change, remove button.
                if (imageList._buttonAdd != null)
                {
                    imageList._buttonAdd.Visibility = Visibility.Visible;
                }
                if (imageList._buttonChange != null)
                {
                    imageList._buttonChange.Visibility = Visibility.Visible;
                }
                if (imageList._buttonRemove != null)
                {
                    imageList._buttonRemove.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region CanUserArrangeImage

        /// <summary>
        /// Gets or sets a value that indicates whether the user can arrange images in the ImageList.
        /// </summary>
        public bool CanUserArrangeImage
        {
            get
            {
                return (bool)GetValue(CanUserArrangeImageProperty);
            }
            set
            {
                SetValue(CanUserArrangeImageProperty, value);
            }
        }

        public static readonly DependencyProperty CanUserArrangeImageProperty =
            DependencyProperty.Register("CanUserArrangeImage", typeof(bool), typeof(ImageList), new UIPropertyMetadata(true, CanUserArrangeImageChanged));

        public static void CanUserArrangeImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageList imageList = d as ImageList;

            if (imageList._listBox == null)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                // Allow arrange image with drag and drop.
                Style listBoxItemContainerStyle = new Style(typeof(ListBoxItem));
                listBoxItemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
                listBoxItemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseMoveEvent, new MouseEventHandler(imageList.ListBoxItemMouseMove)));
                listBoxItemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(imageList.ListBoxItemDrop)));
                imageList._listBox.ItemContainerStyle = listBoxItemContainerStyle;
            }
            else
            {
                // Not allow arrange image.
                imageList._listBox.ItemContainerStyle = null;
            }
        }



        #endregion

        #endregion

        #region DependencyProperties
        /// <summary>
        /// To get , number of image on ImageControl.Default value is 6.
        /// </summary>
        public int MaxNumberOfImages
        {
            get
            {
                return (int)GetValue(MaxNumberOfImagesProperty);
            }
            set
            {
                SetValue(MaxNumberOfImagesProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for MaxNumberOfImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxNumberOfImagesProperty =
            DependencyProperty.Register("MaxNumberOfImages", typeof(int), typeof(ImageList), new UIPropertyMetadata(6));

        #endregion

        #region Methods
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (_listBox != null && _listBox.ItemsSource != null && (_listBox.ItemsSource as IList).Count > 0)
            {
                _listBox.ScrollIntoView((_listBox.ItemsSource as IList)[0]);
            }
        }

        protected override void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
        {
            base.OnDisplayMemberPathChanged(oldDisplayMemberPath, newDisplayMemberPath);

            if (!string.IsNullOrWhiteSpace(newDisplayMemberPath))
            {
                _displayMemberPath = newDisplayMemberPath;

                // Remove dot at head.
                if (_displayMemberPath[0] == _dot)
                {
                    _displayMemberPath = _displayMemberPath.Remove(0, 1);
                }

                // Remove dot at last.
                if (!string.IsNullOrWhiteSpace(_displayMemberPath) &&
                    _displayMemberPath[_displayMemberPath.Length - 1] == _dot)
                {
                    _displayMemberPath = _displayMemberPath.Remove(_displayMemberPath.Length - 1, 1);
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._listBox = GetTemplateChild("PART_ListBox") as ListBox;
            this._image = GetTemplateChild("PART_Image") as Image;
            this._buttonAdd = GetTemplateChild("PART_ButtonAdd") as Button;
            this._buttonChange = GetTemplateChild("PART_ButtonChange") as Button;
            this._buttonRemove = GetTemplateChild("PART_ButtonRemove") as Button;
            this._gridButton = GetTemplateChild("PART_GRIDBUTTON") as Grid;
            this._gridImage = GetTemplateChild("PART_GRIDIMAGE") as Grid;
            if (_listBox != null)
            {
                Binding binding = new Binding("ItemsSource");
                binding.Mode = BindingMode.TwoWay;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ImageList), 1);
                _listBox.SetBinding(ListBox.ItemsSourceProperty, binding);

                binding = new Binding("SelectedValuePath");
                binding.Mode = BindingMode.TwoWay;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ImageList), 1);
                _listBox.SetBinding(ListBox.SelectedValuePathProperty, binding);

                binding = new Binding("SelectedItem");
                binding.Mode = BindingMode.TwoWay;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ImageList), 1);
                _listBox.SetBinding(ListBox.SelectedItemProperty, binding);

                if (CanUserArrangeImage)
                {
                    Style listBoxItemContainerStyle = new Style(typeof(ListBoxItem));
                    listBoxItemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
                    listBoxItemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseMoveEvent, new MouseEventHandler(ListBoxItemMouseMove)));
                    listBoxItemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ListBoxItemDrop)));
                    _listBox.ItemContainerStyle = listBoxItemContainerStyle;
                }

                DataTemplate listBoxItemTemplate = new DataTemplate(typeof(ListBoxItem));
                FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(Image));
                listBoxItemTemplate.VisualTree = frameworkElementFactory;
                binding = new Binding(DisplayMemberPath);
                binding.Mode = BindingMode.OneWay;
                frameworkElementFactory.SetBinding(Image.SourceProperty, binding);
                frameworkElementFactory.SetValue(Image.WidthProperty, (double)25);
                frameworkElementFactory.SetValue(Image.HeightProperty, (double)25);
                frameworkElementFactory.SetValue(Image.StretchProperty, Stretch.Fill);
                _listBox.ItemTemplate = listBoxItemTemplate;

                _listBox.SelectionMode = System.Windows.Controls.SelectionMode.Single;
                _listBox.SelectionChanged += ListBoxSelectionChanged;
                _listBox.Loaded += ListBoxLoaded;
            }

            if (_buttonAdd != null)
            {
                _buttonAdd.Click += ButtonAddClick;
                if (IsReadOnly)
                {
                    _buttonAdd.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            if (_buttonChange != null)
            {
                _buttonChange.Click += ButtonChangeClick;
                if (IsReadOnly)
                {
                    _buttonChange.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            if (_buttonRemove != null)
            {
                _buttonRemove.Click += ButtonRemoveClick;
                if (IsReadOnly)
                {
                    _buttonRemove.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            if (this._gridImage != null)
            {
                this._gridImage.MouseMove += new MouseEventHandler(_gridImage_MouseMove);
                this._gridImage.MouseLeave += new MouseEventHandler(_gridImage_MouseLeave);
            }
            //To set visibility for Control if number of items is 1.
            this.SetVisibleImageList();
        }

        private void _gridImage_MouseLeave(object sender, MouseEventArgs e)
        {
            this._gridButton.Visibility = Visibility.Collapsed;
        }

        void _gridImage_MouseMove(object sender, MouseEventArgs e)
        {
            //To check that active and inactive button.
            this._buttonChange.Visibility = Visibility.Collapsed;
            this._buttonRemove.Visibility = Visibility.Collapsed;
            this._buttonAdd.Visibility = Visibility.Visible;
            if (this._currentItem != null
                && this._buttonChange != null
                && this._buttonRemove != null)
            {
                this._buttonChange.Visibility = Visibility.Visible;
                this._buttonRemove.Visibility = Visibility.Visible;
            }
            if (this.MaxNumberOfImages == 1
                && this._listBox != null
                && this._listBox.Items.Count == 1)
                this._buttonAdd.Visibility = Visibility.Collapsed;
            //To show buttons grid. 
            this._gridButton.Visibility = Visibility.Visible;
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            this._currentItem = null;
            this.SetVisibleImageList();
            if (newValue != null && newValue.Cast<object>().Count() > 0)
            {
                this.SelectCurrentImage(newValue.Cast<object>().First());
            }
            else if (_image != null)
            {
                _image.Source = null;
            }
        }

        private object GetPropertyValue(object obj, string propertyPath)
        {
            if (obj == null)
            {
                throw new NullReferenceException("Object reference not set to an instance of an object.");
            }

            if (propertyPath == null)
            {
                return null;
            }

            PropertyInfo propInfo;
            int indexOfDot = propertyPath.IndexOf(_dot);
            if (indexOfDot == -1)
            {
                propInfo = obj.GetType().GetProperty(propertyPath);
                if (propInfo == null)
                {
                    return null;
                }
                return propInfo.GetValue(obj, null);
            }

            propInfo = obj.GetType().GetProperty(propertyPath.Substring(0, indexOfDot));
            if (propInfo == null)
            {
                return null;
            }

            return GetPropertyValue(propInfo.GetValue(obj, null), propertyPath.Substring(indexOfDot + 1));
        }

        private void SetPropertyValue(object obj, string propertyPath, object value)
        {
            if (obj == null)
            {
                throw new NullReferenceException("Object reference not set to an instance of an object.");
            }

            if (propertyPath == null)
            {
                return;
            }

            PropertyInfo propInfo;
            int indexOfDot = propertyPath.IndexOf(_dot);
            if (indexOfDot == -1)
            {

                propInfo = obj.GetType().GetProperty(propertyPath);
                if (propInfo == null)
                {
                    return;
                }
                if (propInfo.CanWrite)
                {
                    try
                    {
                        propInfo.SetValue(obj, value, null);
                    }
                    catch
                    {
                        throw;
                    }
                }
                else
                {
                    throw new ArgumentException("Property set method not found.");
                }
            }
            else
            {
                propInfo = obj.GetType().GetProperty(propertyPath.Substring(0, indexOfDot));
                if (propInfo == null)
                {
                    return;
                }

                var obj1 = propInfo.GetValue(obj, null);

                if (obj1 == null)
                {
                    if (propInfo.PropertyType.GetConstructor(Type.EmptyTypes) != null)
                    {
                        obj1 = Activator.CreateInstance(propInfo.PropertyType);
                        propInfo.SetValue(obj, obj1, null);
                    }
                    else
                    {
                        throw new MissingMethodException("No parameterless constructor defined for this object.");
                    }
                }

                SetPropertyValue(obj1, propertyPath.Substring(indexOfDot + 1), value);
            }
        }

        /// <summary>
        /// Show current image.
        /// </summary>
        /// <param name="item">Object contains image.</param>
        private void SelectCurrentImage(object item)
        {
            if (item != null)
            {
                if (!string.IsNullOrWhiteSpace(_displayMemberPath))
                {
                    string imageFilePath = GetPropertyValue(item, _displayMemberPath) as String;
                    if (!string.IsNullOrWhiteSpace(imageFilePath) && CheckValidImage(imageFilePath))
                    {
                        // Show current image.
                        if (_image != null)
                        {
                            _image.Source = new BitmapImage(new Uri(imageFilePath));
                        }
                    }
                    else
                    {
                        if (_image != null)
                        {
                            _image.Source = null;
                        }
                    }
                }
                _currentItem = item;
            }
        }

        /// <summary>
        /// Check the input file is an image or not ?
        /// </summary>
        /// <param name="file">Input file for check.</param>
        /// <returns>True if input file is image, else False.</returns>
        private bool CheckValidImage(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    BitmapImage bitmapImage = new BitmapImage(new Uri(file));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Events

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectCurrentImage(_listBox.SelectedItem);
        }

        private void ListBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (_listBox.ItemsSource != null)
            {
                SelectCurrentImage(_listBox.ItemsSource.Cast<object>().FirstOrDefault());
            }
        }

        private void ListBoxItemMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                if (draggedItem != null)
                {
                    DragDrop.DoDragDrop(draggedItem, draggedItem.Content, DragDropEffects.Move);
                }
            }
        }

        private void ListBoxItemDrop(object sender, DragEventArgs e)
        {
            Type itemSourceType = _listBox.ItemsSource.GetType();
            if (itemSourceType.IsArray || itemSourceType.FullName == "System.Collections.ArrayList")
            {
                throw new NotSupportedException("ItemsSource's type is not supported.");
            }

            object targetItem = (sender as ListBoxItem).Content;
            object sourceItem = e.Data.GetData(targetItem.GetType());

            int sourceItemIndex = _listBox.Items.IndexOf(sourceItem);
            int targetItemIndex = _listBox.Items.IndexOf(targetItem);

            if (sourceItemIndex < targetItemIndex)
            {
                (_listBox.ItemsSource as IList).Insert(targetItemIndex + 1, sourceItem);
                (_listBox.ItemsSource as IList).RemoveAt(sourceItemIndex);
            }
            else
            {
                int remIndex = sourceItemIndex + 1;
                if ((_listBox.ItemsSource as IList).Count + 1 > remIndex)
                {
                    (_listBox.ItemsSource as IList).Insert(targetItemIndex, sourceItem);
                    (_listBox.ItemsSource as IList).RemoveAt(remIndex);
                }
            }

            _listBox.SelectedItem = sourceItem;
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            if (this._listBox != null && this._listBox.ItemsSource != null
                && this.CheckNumberOfImage())
            {
                Type itemSourceType = _listBox.ItemsSource.GetType();
                if (!itemSourceType.IsArray
                    && itemSourceType.FullName != "System.Collections.ArrayList")
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Supported images|*.jpg;*.jpeg;*.gif;*.png;*.bmp";
                    openFileDialog.Multiselect = true;
                    openFileDialog.FileOk += delegate
                    {
                        Type elementType = TypeHelper.GetElementType(itemSourceType);
                        if (elementType == null)
                        {
                            throw new NullReferenceException("Object reference not set to an instance of an object.");
                        }
                        if (elementType.GetConstructor(Type.EmptyTypes) != null)
                        {
                            //To remove items in openFileDialog.FileNames if number of files is larger than number of image in MaxNumberOfImage.
                            int numberOfItems = 0;
                            if (this._listBox != null && this._listBox.HasItems)
                                numberOfItems = this._listBox.Items.Count;
                            int numberOfFileNames = openFileDialog.FileNames.Count();
                            if ((numberOfItems + numberOfFileNames) > this.MaxNumberOfImages)
                                numberOfFileNames = this.MaxNumberOfImages - this._listBox.Items.Count;
                            for (int i = 0; i < numberOfFileNames; i++)
                            {
                                if (this.CheckValidImage(openFileDialog.FileNames[i]))
                                {
                                    object newElement = Activator.CreateInstance(elementType);
                                    this.SetPropertyValue(newElement, _displayMemberPath, openFileDialog.FileNames[i]);
                                    (this._listBox.ItemsSource as IList).Add(newElement);
                                }
                            }
                            if (this._listBox.SelectedItem == null)
                            {
                                this.SelectCurrentImage(_listBox.ItemsSource.Cast<object>().FirstOrDefault());
                            }
                        }
                        else
                        {
                            throw new MissingMethodException("No parameterless constructor defined for this object.");
                        }
                    };
                    openFileDialog.ShowDialog();
                }
                else
                {
                    throw new NotSupportedException("ItemsSource's type is not supported.");
                }
            }
        }

        private void ButtonRemoveClick(object sender, RoutedEventArgs e)
        {
            if (_listBox != null
                && _listBox.ItemsSource != null
                && _currentItem != null
                && _listBox.Items.Count > 0)
            {
                Type itemSourceType = _listBox.ItemsSource.GetType();
                if (itemSourceType.IsArray || itemSourceType.FullName == "System.Collections.ArrayList")
                {
                    throw new NotSupportedException("ItemsSource's type is not supported.");
                }

                (_listBox.ItemsSource as IList).Remove(_currentItem);
                if ((_listBox.ItemsSource as IList).Count > 0)
                {
                    this.SelectCurrentImage(_listBox.ItemsSource.Cast<object>().FirstOrDefault());
                    this._listBox.SelectedItem = _currentItem;
                }
                else
                {
                    if (_image != null)
                    {
                        _image.Source = null;
                    }
                    _currentItem = null;
                }
            }
            if (this._buttonAdd != null && this._buttonAdd.Visibility == Visibility.Collapsed)
                this._buttonAdd.Visibility = Visibility.Visible;
        }

        private void ButtonChangeClick(object sender, RoutedEventArgs e)
        {
            if (_currentItem != null
                && _listBox.Items.Count > 0)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Supported images|*.jpg;*.jpeg;*.gif;*.png;*.bmp";
                openFileDialog.Multiselect = false;
                openFileDialog.FileOk += delegate
                {
                    if (this.CheckValidImage(openFileDialog.FileName))
                    {
                        SetPropertyValue(_currentItem, _displayMemberPath, openFileDialog.FileName);
                        if (_image != null)
                        {
                            _image.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                        }
                    }
                };
                openFileDialog.ShowDialog();
            }
        }

        private bool CheckNumberOfImage()
        {
            return (this._listBox != null
                && (!this._listBox.HasItems || this._listBox.Items.Count < this.MaxNumberOfImages));
        }

        private void SetVisibleImageList()
        {
            this._gridContentImage = GetTemplateChild("PART_GRIDLISTBOX") as Grid;
            if (this._gridContentImage != null)
            {
                if (this.MaxNumberOfImages == 1)
                    this._gridContentImage.Visibility = Visibility.Collapsed;
                else
                    this._gridContentImage.Visibility = Visibility.Visible;
            }
        }
        #endregion
    }
}
