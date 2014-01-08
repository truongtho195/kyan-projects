using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.IO;
using CPC.Control.CropImageHelper;
using System.Windows.Documents;

namespace CPC.Control
{
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_StackPannel", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_ButtonRotateLeft", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonRotateRight", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonFlipX", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonFlipY", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonZoomIn", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonZoomOut", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonDrop", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonReset", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonSave", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonExit", Type = typeof(Button))]
    public class ImageViewer : System.Windows.Controls.Control
    {
        #region Fields

        /// <summary>
        /// Reference PART_Canvas.
        /// </summary>
        private Canvas _canvas = null;

        /// <summary>
        /// Reference PART_Image.
        /// </summary>
        private Image _image = null;

        /// <summary>
        /// Reference PART_TextBlock.
        /// </summary>
        private TextBlock _textBlock = null;

        /// <summary>
        /// Reference PART_StackPannel.
        /// </summary>
        private StackPanel _stackPannel = null;

        /// <summary>
        /// Reference PART_ButtonRotateLeft.
        /// </summary>
        private Button _buttonRotateLeft = null;

        /// <summary>
        /// Reference PART_ButtonRotateRight.
        /// </summary>
        private Button _buttonRotateRight = null;

        /// <summary>
        /// Reference PART_ButtonFlipX.
        /// </summary>
        private Button _buttonFlipX = null;

        /// <summary>
        /// Reference PART_ButtonFlipY.
        /// </summary>
        private Button _buttonFlipY = null;

        /// <summary>
        /// Reference PART_ButtonZoomIn.
        /// </summary>
        private Button _buttonZoomIn = null;

        /// <summary>
        /// Reference PART_ButtonZoomOut.
        /// </summary>
        private Button _buttonZoomOut = null;

        /// <summary>
        /// Reference PART_ButtonDrop.
        /// </summary>
        private Button _buttonDrop = null;

        /// <summary>
        /// Reference PART_ButtonReset.
        /// </summary>
        private Button _buttonReset = null;

        /// <summary>
        /// Reference PART_ButtonSave.
        /// </summary>
        private Button _buttonSave = null;

        /// <summary>
        /// Reference PART_ButtonExit.
        /// </summary>
        private Button _buttonExit = null;

        /// <summary>
        /// Position of the mouse pointer before move.
        /// </summary>
        private Point _oldPositionOfMouse = new Point();

        /// <summary>
        /// Position of image before move.
        /// </summary>
        private Point _oldPositionOfImage = new Point();

        private Point _measure = new Point();

        /// <summary>
        /// Value zoom in.
        /// </summary>
        private readonly double _zoomInValue = 0.2;

        /// <summary>
        /// Value zoom out.
        /// </summary>
        private readonly double _zoomOutValue = -0.2;

        /// <summary>
        /// Path of image file.
        /// </summary>
        private string _imageFilePath;

        /// <summary>
        /// Determine file is exist or not.
        /// </summary>
        private bool _isFileExist = false;

        /// <summary>
        /// The rectangle used for drop a image.
        /// </summary>
        private CroppingAdorner _croppingAdorner;

        #endregion

        #region Contructors

        static ImageViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewer), new FrameworkPropertyMetadata(typeof(ImageViewer)));
        }

        #endregion

        #region Properties

        #region Source

        /// <summary>
        /// Gets or sets the ImageSource for the image.
        /// </summary>
        public ImageSource Source
        {
            get
            {
                return (ImageSource)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageViewer), new UIPropertyMetadata(null, new PropertyChangedCallback(SourceChanged)));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageViewer imageViewer = d as ImageViewer;
            if (imageViewer != null)
            {
                // Gets new source.
                if (imageViewer._image != null)
                {
                    imageViewer._image.Source = e.NewValue as ImageSource;
                }

                // Gets image file path.
                if (e.NewValue != null)
                {
                    imageViewer._imageFilePath = imageViewer.GetLocalPath(e.NewValue.ToString());
                }
                else
                {
                    imageViewer._imageFilePath = null;
                }

                // Check file exist.
                if (string.IsNullOrWhiteSpace(imageViewer._imageFilePath))
                {
                    imageViewer._isFileExist = false;
                }
                else
                {
                    imageViewer._isFileExist = File.Exists(imageViewer._imageFilePath);
                }

                // Arrange UI.
                if (imageViewer._image != null && imageViewer._isFileExist)
                {
                    if (imageViewer._canvas != null)
                    {
                        imageViewer._canvas.Visibility = Visibility.Visible;
                    }
                    if (imageViewer._textBlock != null)
                    {
                        imageViewer._textBlock.Visibility = Visibility.Collapsed;
                    }
                    if (imageViewer._stackPannel != null)
                    {
                        imageViewer._stackPannel.IsEnabled = true;
                    }
                }
                else
                {
                    if (imageViewer._canvas != null)
                    {
                        imageViewer._canvas.Visibility = Visibility.Collapsed;
                    }
                    if (imageViewer._textBlock != null)
                    {
                        imageViewer._textBlock.Visibility = Visibility.Visible;
                    }
                    if (imageViewer._stackPannel != null)
                    {
                        imageViewer._stackPannel.IsEnabled = false;
                    }
                }

                imageViewer.FitImage();
            }
        }

        #endregion

        #endregion

        #region Override Methods

        #region OnApplyTemplate

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _canvas = GetTemplateChild("PART_Canvas") as Canvas;
            _image = GetTemplateChild("PART_Image") as Image;
            _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
            _stackPannel = GetTemplateChild("PART_StackPannel") as StackPanel;
            _buttonRotateLeft = GetTemplateChild("PART_ButtonRotateLeft") as Button;
            _buttonRotateRight = GetTemplateChild("PART_ButtonRotateRight") as Button;
            _buttonFlipX = GetTemplateChild("PART_ButtonFlipX") as Button;
            _buttonFlipY = GetTemplateChild("PART_ButtonFlipY") as Button;
            _buttonZoomIn = GetTemplateChild("PART_ButtonZoomIn") as Button;
            _buttonZoomOut = GetTemplateChild("PART_ButtonZoomOut") as Button;
            _buttonDrop = GetTemplateChild("PART_ButtonDrop") as Button;
            _buttonReset = GetTemplateChild("PART_ButtonReset") as Button;
            _buttonSave = GetTemplateChild("PART_ButtonSave") as Button;
            _buttonExit = GetTemplateChild("PART_ButtonExit") as Button;

            if (_canvas != null)
            {
                _canvas.SizeChanged += new SizeChangedEventHandler(CanvasSizeChanged);
            }

            if (_image != null)
            {
                Canvas.SetTop(_image, 0);
                Canvas.SetLeft(_image, 0);

                _image.Source = Source;
                _image.RenderTransform = new ScaleTransform();

                // Events used for move image on canvas.
                _image.MouseLeftButtonDown += new MouseButtonEventHandler(ImageMouseLeftButtonDown);
                _image.MouseLeftButtonUp += new MouseButtonEventHandler(ImageMouseLeftButtonUp);
                _image.MouseMove += new MouseEventHandler(ImageMouseMove);

                // Event used for zoom image on canvas.
                _image.MouseWheel += new MouseWheelEventHandler(ImageMouseWheel);
            }

            if (_buttonRotateLeft != null)
            {
                // Event used for rotate image on canvas.
                _buttonRotateLeft.Click += new RoutedEventHandler(ButtonRotateLeftClick);
            }

            if (_buttonRotateRight != null)
            {
                // Event used for rotate image on canvas.
                _buttonRotateRight.Click += new RoutedEventHandler(ButtonRotateRightClick);
            }

            if (_buttonFlipX != null)
            {
                // Event used for flip image on canvas.
                _buttonFlipX.Click += new RoutedEventHandler(ButtonFlipXClick);
            }

            if (_buttonFlipY != null)
            {
                // Event used for flip image on canvas.
                _buttonFlipY.Click += new RoutedEventHandler(ButtonFlipYClick);
            }

            if (_buttonZoomIn != null)
            {
                // Event used for zoom image on canvas.
                _buttonZoomIn.Click += new RoutedEventHandler(ButtonZoomInClick);
            }

            if (_buttonZoomOut != null)
            {
                // Event used for zoom image on canvas.
                _buttonZoomOut.Click += new RoutedEventHandler(ButtonZoomOutClick);
            }

            if (_buttonDrop != null)
            {
                // Event used for drop image on canvas.
                _buttonDrop.Click += new RoutedEventHandler(ButtonDropClick);
            }

            if (_buttonReset != null)
            {
                // Event used for fit image on canvas.
                _buttonReset.Click += new RoutedEventHandler(ButtonResetClick);
            }

            if (_buttonSave != null)
            {
                // Event save image after drop.
                _buttonSave.Click += new RoutedEventHandler(ButtonSaveClick);
            }

            if (_buttonExit != null)
            {
                // Event exit drop an image.
                _buttonExit.Click += new RoutedEventHandler(ButtonExitClick);
            }

            // Arrange UI.
            if (_image != null && _isFileExist)
            {
                if (_canvas != null)
                {
                    _canvas.Visibility = Visibility.Visible;
                }
                if (_textBlock != null)
                {
                    _textBlock.Visibility = Visibility.Collapsed;
                }
                if (_stackPannel != null)
                {
                    _stackPannel.IsEnabled = true;
                }
            }
            else
            {
                if (_canvas != null)
                {
                    _canvas.Visibility = Visibility.Collapsed;
                }
                if (_textBlock != null)
                {
                    _textBlock.Visibility = Visibility.Visible;
                }
                if (_stackPannel != null)
                {
                    _stackPannel.IsEnabled = false;
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        #region GetLocalPath

        /// <summary>
        /// Gets a local operating-system representation of a file name.
        /// </summary>
        private string GetLocalPath(string filePath)
        {
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage.UriSource = new Uri(filePath);
                bitmapImage.EndInit();
                return bitmapImage.UriSource.LocalPath;
            }
            catch
            {
                return filePath;
            }
        }

        #endregion

        #region AlignImageCenterCanvas

        /// <summary>
        /// Align center control image in canvas.
        /// </summary>
        private void AlignImageCenterCanvas()
        {
            if (_image != null && Double.IsNaN(_image.Width) && _canvas != null)
            {
                _image.Width = _canvas.ActualWidth;
                _image.Height = _canvas.ActualHeight;
            }
        }

        #endregion

        #region CreateClip

        /// <summary>
        /// Create clip for canvas.
        /// </summary>
        private void CreateClip()
        {
            if (_canvas != null)
            {
                _canvas.Clip = new RectangleGeometry(new Rect(0, 0, _canvas.ActualWidth, _canvas.ActualHeight));
            }
        }

        #endregion

        #region EncodeAndSaveImage

        /// <summary>
        /// Save image.
        /// </summary>
        /// <param name="bitmapSource">BitmapSource to save.</param>
        /// <param name="filePath">File path to save.</param>
        /// <returns>True if success.</returns>
        private bool EncodeAndSaveImage(BitmapSource bitmapSource, string filePath)
        {
            bool isSuccess = false;
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                switch (fileInfo.Extension.ToLower())
                {
                    case ".png":
                        PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                        pngBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        using (System.IO.FileStream file = System.IO.File.OpenWrite(filePath))
                        {
                            pngBitmapEncoder.Save(file);
                        }
                        break;
                    case ".bmp":
                        BmpBitmapEncoder bmpBitmapEncoder = new BmpBitmapEncoder();
                        bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        using (System.IO.FileStream file = System.IO.File.OpenWrite(filePath))
                        {
                            bmpBitmapEncoder.Save(file);
                        }
                        break;
                    case ".gif":
                        GifBitmapEncoder gifBitmapEncoder = new GifBitmapEncoder();
                        gifBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        using (System.IO.FileStream file = System.IO.File.OpenWrite(filePath))
                        {
                            gifBitmapEncoder.Save(file);
                        }
                        break;
                    case ".tif":
                        TiffBitmapEncoder tiffBitmapEncoder = new TiffBitmapEncoder();
                        tiffBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        using (System.IO.FileStream file = System.IO.File.OpenWrite(filePath))
                        {
                            tiffBitmapEncoder.Save(file);
                        }
                        break;
                    default:
                        JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
                        jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        using (System.IO.FileStream file = System.IO.File.OpenWrite(filePath))
                        {
                            jpegBitmapEncoder.Save(file);
                        }
                        break;
                }

                isSuccess = true;
            }
            catch
            {
                throw;
            }

            return isSuccess;
        }

        #endregion

        #region FitImage

        /// <summary>
        /// Reset image to fit size.
        /// </summary>
        private void FitImage()
        {
            if (_image != null && _isFileExist)
            {
                ScaleTransform scaleTransform = _image.RenderTransform as ScaleTransform;
                scaleTransform.ScaleX = 1;
                scaleTransform.ScaleY = 1;

                if (_canvas != null)
                {
                    if (!Double.IsNaN(_image.Width))
                    {
                        _image.Width = _canvas.ActualWidth;
                        _image.Height = _canvas.ActualHeight;
                    }

                    _image.SetValue(Canvas.LeftProperty, 0D);
                    _image.SetValue(Canvas.TopProperty, 0D);
                }
            }
        }

        #endregion

        #region Zoom

        /// <summary>
        /// Zoom image.
        /// </summary>
        /// <param name="isZoomIn">True zoom in. False zoom out.</param>
        private void Zoom(bool isZoomIn)
        {
            if (_image == null || !_isFileExist)
            {
                return;
            }

            ScaleTransform scaleTransform = _image.RenderTransform as ScaleTransform;

            if (isZoomIn)
            {
                scaleTransform.ScaleX += _zoomInValue;
                scaleTransform.ScaleY += _zoomInValue;
            }
            else
            {
                if (scaleTransform.ScaleX > 1 && scaleTransform.ScaleY > 1)
                {
                    scaleTransform.ScaleX += _zoomOutValue;
                    scaleTransform.ScaleY += _zoomOutValue;
                }
            }
        }

        #endregion

        #region Rotate

        /// <summary>
        /// Rotate image.
        /// </summary>
        /// <param name="isRotateLeft">True rotate left. False rotate right.</param>
        private void Rotate(bool isRotateLeft)
        {
            try
            {
                if (_image == null || !_isFileExist)
                {
                    return;
                }

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(_imageFilePath);
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                if (isRotateLeft)
                {
                    bitmapImage.Rotation = Rotation.Rotate270;
                }
                else
                {
                    bitmapImage.Rotation = Rotation.Rotate90;
                }
                bitmapImage.EndInit();
                EncodeAndSaveImage(bitmapImage, _imageFilePath);

                //Refresh image after rotate.
                _image.Source = bitmapImage;
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message);
            }
        }

        #endregion

        #region Flip

        private void Flip(bool isHorizontal)
        {
            try
            {
                if (_image == null || !_isFileExist)
                {
                    return;
                }

                ScaleTransform scaleTransform;
                if (isHorizontal)
                {
                    scaleTransform = new ScaleTransform(-1, 1);
                }
                else
                {
                    scaleTransform = new ScaleTransform(1, -1);
                }

                TransformedBitmap transformedBitmap = new TransformedBitmap();
                transformedBitmap.BeginInit();
                transformedBitmap.Source = _image.Source as BitmapImage;
                transformedBitmap.Transform = scaleTransform;
                transformedBitmap.EndInit();

                EncodeAndSaveImage(transformedBitmap, _imageFilePath);

                //Refresh image after flip.
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(_imageFilePath);
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                _image.Source = bitmapImage;
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message);
            }
        }

        #endregion

        #region ChangeDropMode

        /// <summary>
        /// Change drop mode.
        /// </summary>
        /// <param name="isDropMode">True in drop mode. False out drop mode.</param>
        private void ChangeDropMode(bool isDropMode)
        {
            if (_image == null || !_isFileExist || _canvas == null)
            {
                return;
            }

            if (isDropMode)
            {
                FitImage();

                AddCropToElement(_image);

                _image.MouseLeftButtonDown -= ImageMouseLeftButtonDown;
                _image.MouseLeftButtonUp -= ImageMouseLeftButtonUp;
                _image.MouseMove -= ImageMouseMove;
                _image.MouseWheel -= ImageMouseWheel;

                if (_buttonRotateLeft != null)
                {
                    _buttonRotateLeft.Visibility = Visibility.Collapsed;
                }
                if (_buttonRotateRight != null)
                {
                    _buttonRotateRight.Visibility = Visibility.Collapsed;
                }
                if (_buttonZoomIn != null)
                {
                    _buttonZoomIn.Visibility = Visibility.Collapsed;
                }
                if (_buttonZoomOut != null)
                {
                    _buttonZoomOut.Visibility = Visibility.Collapsed;
                }
                if (_buttonReset != null)
                {
                    _buttonReset.Visibility = Visibility.Collapsed;
                }
                if (_buttonDrop != null)
                {
                    _buttonDrop.Visibility = Visibility.Collapsed;
                }
                if (_buttonFlipX != null)
                {
                    _buttonFlipX.Visibility = Visibility.Collapsed;
                }
                if (_buttonFlipY != null)
                {
                    _buttonFlipY.Visibility = Visibility.Collapsed;
                }
                if (_buttonSave != null)
                {
                    _buttonSave.Visibility = Visibility.Visible;
                }
                if (_buttonExit != null)
                {
                    _buttonExit.Visibility = Visibility.Visible;
                }
            }
            else
            {
                RemoveCrop(_image);

                _image.MouseLeftButtonDown += ImageMouseLeftButtonDown;
                _image.MouseLeftButtonUp += ImageMouseLeftButtonUp;
                _image.MouseMove += ImageMouseMove;
                _image.MouseWheel += ImageMouseWheel;

                if (_buttonRotateLeft != null)
                {
                    _buttonRotateLeft.Visibility = Visibility.Visible;
                }
                if (_buttonRotateRight != null)
                {
                    _buttonRotateRight.Visibility = Visibility.Visible;
                }
                if (_buttonZoomIn != null)
                {
                    _buttonZoomIn.Visibility = Visibility.Visible;
                }
                if (_buttonZoomOut != null)
                {
                    _buttonZoomOut.Visibility = Visibility.Visible;
                }
                if (_buttonReset != null)
                {
                    _buttonReset.Visibility = Visibility.Visible;
                }
                if (_buttonDrop != null)
                {
                    _buttonDrop.Visibility = Visibility.Visible;
                }
                if (_buttonFlipX != null)
                {
                    _buttonFlipX.Visibility = Visibility.Visible;
                }
                if (_buttonFlipY != null)
                {
                    _buttonFlipY.Visibility = Visibility.Visible;
                }
                if (_buttonSave != null)
                {
                    _buttonSave.Visibility = Visibility.Collapsed;
                }
                if (_buttonExit != null)
                {
                    _buttonExit.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion

        #region CalculateMeasure

        private void CalculateMeasure()
        {
            if (_image != null && _canvas != null && _image.Source != null)
            {
                double canvasPercent = _canvas.ActualWidth / _canvas.ActualHeight;
                double imagePercent = _image.Source.Width / _image.Source.Height;

                if (canvasPercent > imagePercent)
                {
                    _measure.X = (_canvas.ActualWidth - (_canvas.ActualHeight * imagePercent)) / 2;
                    _measure.Y = 0;
                }
                else
                {
                    _measure.X = 0;
                    _measure.Y = (_canvas.ActualHeight - (_canvas.ActualWidth / imagePercent)) / 2;
                }
            }
        }

        #endregion

        #region AddCropToElement

        /// <summary>
        /// Add CroppingAdorner on an element.
        /// </summary>
        /// <param name="frameworkElement">Element to add CroppingAdorner.</param>
        private void AddCropToElement(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
            {
                return;
            }

            CalculateMeasure();

            // Draw rectangle on Image.
            Rect rcInterior = new Rect(
                (double)frameworkElement.GetValue(Canvas.LeftProperty),
                (double)frameworkElement.GetValue(Canvas.TopProperty),
                frameworkElement.ActualWidth,
                frameworkElement.ActualHeight);

            AdornerLayer aly = AdornerLayer.GetAdornerLayer(frameworkElement);
            _croppingAdorner = new CroppingAdorner(frameworkElement, rcInterior);
            _croppingAdorner.Distance = _measure;
            aly.Add(_croppingAdorner);
        }

        #endregion

        #region RemoveCrop

        /// <summary>
        /// Remove CroppingAdorner on an element.
        /// </summary>
        /// <param name="frameworkElement">Element to remove CroppingAdorner.</param>
        private void RemoveCrop(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
            {
                return;
            }

            AdornerLayer aly = AdornerLayer.GetAdornerLayer(frameworkElement);
            aly.Remove(_croppingAdorner);
        }

        #endregion

        #region DropAndSaveImage

        /// <summary>
        /// Drop and save image.
        /// </summary>
        private void DropAndSaveImage()
        {
            try
            {
                if (_croppingAdorner == null || _image == null || !_isFileExist)
                {
                    return;
                }

                BitmapSource bitmapSource = _croppingAdorner.BpsCrop();
                if (bitmapSource != null)
                {
                    EncodeAndSaveImage(bitmapSource, _imageFilePath);

                    // Refresh image after drop.
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(_imageFilePath);
                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    _image.Source = bitmapImage;

                    ChangeDropMode(false);
                }
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message);
            }
        }

        #endregion

        #endregion

        #region Events

        #region CanvasSizeChanged

        private void CanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlignImageCenterCanvas();
            CreateClip();
            FitImage();
        }

        #endregion

        #region Move Image

        #region ImageMouseLeftButtonDown

        private void ImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _image.CaptureMouse();
            Mouse.OverrideCursor = Cursors.Hand;

            // Get position of image and mouse pointer.
            if (_canvas != null)
            {
                _oldPositionOfImage.X = (double)_image.GetValue(Canvas.LeftProperty);
                _oldPositionOfImage.Y = (double)_image.GetValue(Canvas.TopProperty);

                _oldPositionOfMouse = e.GetPosition(_canvas);
            }
        }

        #endregion

        #region ImageMouseLeftButtonUp

        private void ImageMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _image.ReleaseMouseCapture();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        #endregion

        #region ImageMouseMove

        private void ImageMouseMove(object sender, MouseEventArgs e)
        {
            if (!_image.IsMouseCaptured || _canvas == null)
            {
                return;
            }

            // Get new position of the mouse pointer when move.
            Point newPosition = e.GetPosition(_canvas);

            // Move image.
            Point distance = new Point();
            distance.X = _oldPositionOfImage.X + (newPosition.X - _oldPositionOfMouse.X);
            distance.Y = _oldPositionOfImage.Y + (newPosition.Y - _oldPositionOfMouse.Y);

            _image.SetValue(Canvas.LeftProperty, distance.X);
            _image.SetValue(Canvas.TopProperty, distance.Y);
        }

        #endregion

        #endregion

        #region Zoom Image

        #region ImageMouseWheel

        private void ImageMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Zoom(e.Delta > 0);
        }

        #endregion

        #region ButtonZoomInClick

        private void ButtonZoomInClick(object sender, RoutedEventArgs e)
        {
            Zoom(true);
        }

        #endregion

        #region ButtonZoomOutClick

        private void ButtonZoomOutClick(object sender, RoutedEventArgs e)
        {
            Zoom(false);
        }

        #endregion

        #endregion

        #region Rotate Image

        #region ButtonRotateLeftClick

        private void ButtonRotateLeftClick(object sender, RoutedEventArgs e)
        {
            Rotate(true);
        }

        #endregion

        #region ButtonRotateRightClick

        private void ButtonRotateRightClick(object sender, RoutedEventArgs e)
        {
            Rotate(false);
        }

        #endregion

        #endregion

        #region Flip Image

        #region ButtonFlipXClick

        private void ButtonFlipXClick(object sender, RoutedEventArgs e)
        {
            Flip(true);
        }

        #endregion

        #region ButtonFlipYClick

        private void ButtonFlipYClick(object sender, RoutedEventArgs e)
        {
            Flip(false);
        }

        #endregion

        #endregion

        #region Drop Image

        #region ButtonDropClick

        private void ButtonDropClick(object sender, RoutedEventArgs e)
        {
            ChangeDropMode(true);
        }

        #endregion

        #region ButtonSaveClick

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            DropAndSaveImage();
        }

        #endregion

        #region ButtonExitClick

        private void ButtonExitClick(object sender, RoutedEventArgs e)
        {
            ChangeDropMode(false);
        }

        #endregion

        #endregion

        #region Fit Image

        private void ButtonResetClick(object sender, RoutedEventArgs e)
        {
            FitImage();
        }

        #endregion

        #endregion
    }
}
