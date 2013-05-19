///////////////
//////******** Version 1.1 , Created by Thaipn.CPC *********
///////////////
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections;
using System.Threading;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Specialized;
namespace CPC.Control
{
    public enum ImageBoxControlPropertyName
    {
        LargePhotoPath = 0,
        PhotoModel = 1,
        IsDirty = 2,
        LargePhotoFilename = 3
    }
    /// <summary>
    /// Interaction logic for ImageBoxControl.xaml
    /// </summary>
    public partial class ImageBoxControl : UserControl, INotifyPropertyChanged
    {
        #region ConstrcpcControltor
        public ImageBoxControl()
        {
            InitializeComponent();
            this.btnSelectImage.Click += new RoutedEventHandler(btnSelectImage_Click);
            this.lstBoxImage.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(lstBoxImage_PreviewMouseLeftButtonDown);
            this.lstBoxImage.DragLeave += new DragEventHandler(ImageBoxControl_DragLeave);
            this.lstBoxImage.Drop += new DragEventHandler(ImageBoxControl_Drop);
            this.lstBoxImage.PreviewKeyDown += new KeyEventHandler(lstBoxImage_PreviewKeyDown);
        }
        #endregion

        #region Fields
        private bool _isDrag = false;
        #endregion

        #region DependencyProperty

        /// <summary>
        /// Get Items for Control
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSourceImagePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ImageBoxControl), new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue != null)
                {
                    ImageBoxControl imageBoxControl = (ImageBoxControl)d;
                    imageBoxControl.SetValueImageCollection(e.NewValue as IEnumerable);
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<OnItemSourceChanged>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        //
        // Summary:
        //     Gets or sets the identifying properties id of the image. This is a
        //     dependency property.
        //
        // Returns:
        //     The name of the element. The default is an empty string.
        public string MyPropertyOfImageID
        {
            get { return (string)GetValue(MyPropertyOfImageIDProperty); }
            set { SetValue(MyPropertyOfImageIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyIDOfImageID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyOfImageIDProperty =
            DependencyProperty.Register("MyPropertyOfImageID", typeof(string), typeof(ImageBoxControl), new UIPropertyMetadata(string.Empty));


        //
        // Summary:
        //     Gets or sets the identifying properties name of the image. This is a
        //     dependency property.
        //
        // Returns:
        //     The name of the element. The default is an empty string.
        public string MyPropertyOfImageName
        {
            get { return (string)GetValue(MyPropertyOfImageNameProperty); }
            set { SetValue(MyPropertyOfImageNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyNameOfImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyOfImageNameProperty =
            DependencyProperty.Register("MyPropertyOfImageName", typeof(string), typeof(ImageBoxControl), new UIPropertyMetadata(string.Empty));

        #endregion

        #region Properties

        #region SetlectedItem

        /// <summary>
        /// SetlectedItem in ListImage
        /// </summary>
        private object _selectedImage;
        public object SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                if (_selectedImage != value && value != null)
                {
                    _selectedImage = value;
                    RaisePropertyChanged(() => SelectedImage);
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
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

        #endregion

        #region EventControl
        private void lstBoxImage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ///Code
            if (e.Key == Key.Right)
                if (((ListBox)sender).SelectedIndex == 5)
                {
                    ((ListBox)sender).SelectedIndex = 0;
                    ((ListBox)sender).Focus();
                }
        }

        #region Drag && Drop
        /// <summary>
        /// Drop item in ListImage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        private void ImageBoxControl_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (!_isDrag) return;
                ListBoxItem row = ItemsControl.ContainerFromElement((ListBox)sender, e.OriginalSource as DependencyObject) as ListBoxItem;
                ///Drag ,drop in item
                if (this.lstBoxImage.SelectedItem == row.Content)
                {
                    this._isDrag = false;
                    return;
                }
                if (row != null)
                {
                    int IndexDrag = this.lstBoxImage.Items.IndexOf(this.lstBoxImage.SelectedItem);
                    int IndexDrop = this.lstBoxImage.Items.IndexOf(row.Content);
                    object ItemDrop = ImageBoxControl.DeepClone(row.Content);
                    object ItemDrag = ImageBoxControl.DeepClone(this.lstBoxImage.SelectedItem);
                    this.SelectedImage = null;
                    if (ItemDrop != null)
                    {
                        foreach (var item in this.lstBoxImage.ItemsSource)
                            if (int.Parse(item.GetType().GetProperty(MyPropertyOfImageID).GetValue(item, null).ToString())
                                == IndexDrag)
                            {
                                item.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(item, null).GetType().GetProperty(MyPropertyOfImageName).SetValue(item.GetType().GetProperty("PhotoModel").GetValue(item, null), ItemDrop.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(ItemDrop, null).GetType().GetProperty(MyPropertyOfImageName).GetValue(ItemDrop.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(ItemDrop, null), null), null);
                                item.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(item, null).GetType().GetProperty(ImageBoxControlPropertyName.LargePhotoFilename.ToString()).SetValue(item.GetType().GetProperty("PhotoModel").GetValue(item, null), ItemDrop.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(ItemDrop, null).GetType().GetProperty(ImageBoxControlPropertyName.LargePhotoFilename.ToString()).GetValue(ItemDrop.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(ItemDrop, null), null), null);
                                item.GetType().GetProperty(ImageBoxControlPropertyName.IsDirty.ToString()).SetValue(item, true, null);
                                break;
                            }
                    }
                    if (ItemDrag != null)
                    {
                        foreach (var item in this.lstBoxImage.ItemsSource)
                            if (int.Parse(item.GetType().GetProperty(MyPropertyOfImageID).GetValue(item, null).ToString())
                                == IndexDrop)
                            {
                                item.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(item, null).GetType().GetProperty(MyPropertyOfImageName).SetValue(item.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(item, null), ItemDrag.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(ItemDrag, null).GetType().GetProperty(MyPropertyOfImageName).GetValue(ItemDrag.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(ItemDrag, null), null), null);
                                item.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(item, null).GetType().GetProperty(ImageBoxControlPropertyName.LargePhotoFilename.ToString()).SetValue(item.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(item, null), ItemDrag.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(ItemDrag, null).GetType().GetProperty(ImageBoxControlPropertyName.LargePhotoFilename.ToString()).GetValue(ItemDrag.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(ItemDrag, null), null), null);
                                item.GetType().GetProperty(ImageBoxControlPropertyName.IsDirty.ToString()).SetValue(item, true, null);
                                this.SelectedImage = item;
                                break;
                            }
                    }
                    //Set focus item
                    (this.lstBoxImage.ItemContainerGenerator.ContainerFromItem(this.SelectedImage) as ListBoxItem).Focus();
                    this._isDrag = false;
                }
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<< ImageBoxControl_Drop >>>>>>>>>" + ex.ToString());
            }
        }

        /// <summary>
        /// Set drag item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageBoxControl_DragLeave(object sender, DragEventArgs e)
        {
            this._isDrag = true;
        }
        /// <summary>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over this element.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstBoxImage_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                ListBoxItem row = ItemsControl.ContainerFromElement((ListBox)sender, e.OriginalSource as DependencyObject) as ListBoxItem;
                if (row != null)
                {
                    row.Focus();
                    row.IsSelected = true;
                    this.SelectedImage = row.Content;
                    DragDrop.DoDragDrop((ListBox)sender, row.Content, DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<< LstBoxImage_PreviewMouseLeftButtonDown >>>>>>>>>" + ex.ToString());
            }
        }

        #endregion
        /// <summary>
        /// SelectedItem in ListImage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.SelectedImage == null) return;
                Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
                openDialog.Filter = "Supported images|*.jpg;*.jpeg;*.gif;*.png;*.bmp";
                openDialog.Multiselect = false;
                openDialog.FileOk += delegate
                {
                    this.SelectedImage.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(this.SelectedImage, null).GetType().GetProperty(MyPropertyOfImageName).SetValue(this.SelectedImage.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(this.SelectedImage, null), openDialog.FileName, null);
                    this.SelectedImage.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(this.SelectedImage, null).GetType().GetProperty(ImageBoxControlPropertyName.LargePhotoFilename.ToString()).SetValue(this.SelectedImage.GetType().GetProperty(ImageBoxControlPropertyName.PhotoModel.ToString()).GetValue(this.SelectedImage, null), openDialog.SafeFileName, null);
                    this.SelectedImage.GetType().GetProperty(ImageBoxControlPropertyName.IsDirty.ToString()).SetValue(this.SelectedImage, true, null);
                };
                openDialog.ShowDialog();
                //Set focus item
                if (this.SelectedImage != null)
                    (this.lstBoxImage.ItemContainerGenerator.ContainerFromItem(this.SelectedImage) as ListBoxItem).Focus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<< btnSelectImage_Click >>>>>>>>>>>>>>>" + ex.Message);
            }
        }

        /// <summary>
        /// Delete item in ListImage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mniDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SelectedImage.GetType().GetProperty("PhotoModel").GetValue(this.SelectedImage, null).GetType().GetProperty(MyPropertyOfImageName).SetValue(this.SelectedImage.GetType().GetProperty("PhotoModel").GetValue(this.SelectedImage, null), string.Empty, null);
                this.SelectedImage.GetType().GetProperty("IsDirty").SetValue(this.SelectedImage, true, null);
                this.SelectedImage.GetType().GetProperty("IsDelete").SetValue(this.SelectedImage, true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<< mniDeleteImage_Click >>>>>>>>>>>>>>>" + ex.Message);
            }
        }

        #endregion

        #region Methods

        #region DeepClone
        /// <summary>
        /// Copy value 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object DeepClone(object obj)
        {
            object objResult = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return objResult;
        }
        #endregion

        #region SetValueImageCollection
        private void SetValueImageCollection(IEnumerable collection)
        {
            try
            {
                //Register event when ItemSource change
                //((INotifyCollectionChanged)this.ItemsSource).CollectionChanged += new NotifyCollectionChangedEventHandler(ImageBoxControl_CollectionChanged);
                //Set item for control
                if (collection != null && collection.Cast<object>().ToList().Count > 0)
                {
                    this.SelectedImage = this.ItemsSource.Cast<object>().ToList()[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("<<<<<<<<<<<<<<< SetValueImageCollection >>>>>>>>>>>>>>>" + ex.Message);
            }
            //this.ItemsSource.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
            //if (newValue.Count > 0)
            //    this.AddItems(newValue);

            //this.Dispatcher.BeginInvoke(
            //              DispatcherPriority.Input,
            //              (ThreadStart)delegate
            //              {
            //                  if (collection != null && collection.Cast<object>().ToList().Count > 0)
            //                  {
            //                      try
            //                      {
            //                          //Set default image
            //                          this.SelectedImage = this.ItemsSource.Cast<object>().ToList()[0];
            //                          //Set focus item                                  
            //                          //(lstBoxImage.ItemContainerGenerator.ContainerFromItem(this.SelectedImage) as ListBoxItem).Focus();
            //                      }
            //                      catch (Exception ex)
            //                      {
            //                          Debug.WriteLine("<<<<<<<<<<<<<<< Dispatcher SelectedItem >>>>>>>>>>>>>>>" + ex.Message);
            //                      }
            //                  }
            //              });
        }

        void ImageBoxControl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }
        #endregion

        #region AddItems
        //Add items for control when itemsource changed
        private void AddItems()
        {

        }

        #endregion

        #region ResetItems
        private void ResetItems()
        {

        }
        #endregion

        #endregion

    }

}
