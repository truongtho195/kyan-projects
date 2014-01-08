using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using CPC.Control;
using CPC.Toolkit.Base;

namespace CPC.Toolkit.Layout
{
    public class BorderLayoutTarget : Border
    {
        #region Properties

        /// <summary>
        /// Gets or sets the size of target
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the position of target
        /// </summary>
        public Point Position { get; set; }

        #endregion
    }

    public class BorderLayoutHost : Border
    {
        #region Defines

        /// <summary>
        /// Kept position of host to animate
        /// </summary>
        private TranslateTransform _translate;

        /// <summary>
        /// Time for animate
        /// </summary>
        private int _animationTime = 700;

        private Storyboard _storyboard = new Storyboard();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the target of host
        /// </summary>
        public BorderLayoutTarget Target { get; set; }

        /// <summary>
        /// Gets or sets the animation is completed
        /// </summary>
        public bool IsAnimationCompleted { get; set; }

        /// <summary>
        /// Gets content of host as ContainerView
        /// </summary>
        public ContainerView Container
        {
            get
            {
                return this.Child != null ? this.Child as ContainerView : null;
            }
        }

        /// <summary>
        /// Gets UserControl in host
        /// </summary>
        public UserControl View
        {
            get
            {
                UserControl view = null;
                if (this.Container != null && this.Container.grdContent.Children.Count > 0)
                    view = this.Container.grdContent.Children[0] as UserControl;
                return view;
            }
        }

        public ViewModelBase ViewModelBase
        {
            get
            {
                return this.Container != null ? this.DataContext as ViewModelBase : null;
            }
        }

        /// <summary>
        /// Gets or sets AllowScreenShot
        /// </summary>
        public bool AllowScreenShot { get; set; }

        /// <summary>
        /// Gets or sets the KeyName
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Gets or sets the HostName
        /// </summary>
        public string DisplayName { get; set; }

        public bool IsOpenList { get; set; }

        public bool IsRefreshData { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public BorderLayoutHost()
        {
            _translate = new TranslateTransform(0, 0);
            this.RenderTransform = _translate;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.AllowScreenShot = false;

            //_storyboard.AccelerationRatio = 1;
            _storyboard.DecelerationRatio = 1;
            _storyboard.Completed += new EventHandler(Storyboard_Completed);
            for (int i = 0; i < 4; i++)
                _storyboard.Children.Add(new DoubleAnimation());
        }

        public BorderLayoutHost(BorderLayoutTarget target)
            : this()
        {
            IsRefreshData = true;
            this.Target = target;
            //this.Target.Loaded += new RoutedEventHandler(Target_Loaded);
            this.Target.LayoutUpdated += new EventHandler(Target_LayoutUpdated);
        }

        #endregion

        #region Methods

        public void SetHostName(string name)
        {
            if (name.EndsWith("List"))
            {
                IsOpenList = true;
                name = name.Substring(0, name.Length - 4);
            }
            DisplayName = name;
            KeyName = name.Replace(" ", "");
        }

        public bool ShowNotification(bool isClosing)
        {
            return (this.DataContext as Toolkit.Base.ViewModelBase).ViewChangingCommand.CanExecute(isClosing);
        }

        /// <summary>
        /// Rotate the title bar of host
        /// </summary>
        /// <param name="isRotateCounterClockwise"></param>
        public void RotateItem(bool isRotateCounterClockwise)
        {
            Border bdTitle = this.Container.bdTitle;
            RotateTransform rotate = new RotateTransform();
            bdTitle.LayoutTransform = rotate;
            if (isRotateCounterClockwise)
            {
                Grid.SetRow(bdTitle, 1);
                Grid.SetColumn(bdTitle, 0);
                rotate.Angle = -90;
            }
            else
            {
                Grid.SetRow(bdTitle, 0);
                Grid.SetColumn(bdTitle, 1);
            }
        }

        /// <summary>
        /// Animate to target
        /// </summary>
        /// <param name="animationTime"></param>
        private void AnimationToTarget(int animationTime)
        {
            IsAnimationCompleted = false;
            //if (IsRefreshData && ViewModelBase != null && CPC.POS.Define.DisplayLoading)
            //    ViewModelBase.IsBusy = true;

            double width = double.IsNaN(Width) ? 0 : Width;
            double height = double.IsNaN(Height) ? 0 : Height;

            // Get current position of host
            TranslateTransform translate = RenderTransform as TranslateTransform;

            // Update the new size and position of target
            Target.Size = new Size(Target.ActualWidth, Target.ActualHeight);
            Target.Position = Target.TranslatePoint(new Point(0, 0), Parent as UIElement);

            //BeginAnimation(BorderLayoutHost.WidthProperty, SetupDoubleAnimation(width, Target.Size.Width, animationTime));
            //BeginAnimation(BorderLayoutHost.HeightProperty, SetupDoubleAnimation(height, Target.Size.Height, animationTime));

            //translate.BeginAnimation(TranslateTransform.XProperty, SetupDoubleAnimation(translate.X, Target.Position.X, animationTime));
            //translate.BeginAnimation(TranslateTransform.YProperty, SetupDoubleAnimation(translate.Y, Target.Position.Y, animationTime));
            Duration duration = TimeSpan.FromMilliseconds(animationTime);
            // Create Width double animation
            DoubleAnimation widthDoubleAnimation = _storyboard.Children[0] as DoubleAnimation;
            widthDoubleAnimation.From = width;
            widthDoubleAnimation.To = Target.Size.Width;
            widthDoubleAnimation.Duration = duration;
            //DoubleAnimation widthDoubleAnimation = SetupDoubleAnimation(width, Target.Size.Width, animationTime);
            SetTargetProperty(widthDoubleAnimation, BorderLayoutHost.WidthProperty.Name);

            // Create Height double animation
            DoubleAnimation heightDoubleAnimation = _storyboard.Children[1] as DoubleAnimation;
            heightDoubleAnimation.From = height;
            heightDoubleAnimation.To = Target.Size.Height;
            heightDoubleAnimation.Duration = duration;
            //DoubleAnimation heightDoubleAnimation = SetupDoubleAnimation(height, Target.Size.Height, animationTime);
            SetTargetProperty(heightDoubleAnimation, BorderLayoutHost.HeightProperty.Name);

            // Create TranslateX double animation
            DoubleAnimation translateXDoubleAnimation = _storyboard.Children[2] as DoubleAnimation;
            translateXDoubleAnimation.From = translate.X;
            translateXDoubleAnimation.To = Target.Position.X;
            translateXDoubleAnimation.Duration = duration;
            //DoubleAnimation translateXDoubleAnimation = SetupDoubleAnimation(translate.X, Target.Position.X, animationTime);
            SetTargetProperty(translateXDoubleAnimation, BorderLayoutHost.RenderTransformProperty.Name + "." + TranslateTransform.XProperty.Name);

            // Create TranslateY double animation
            DoubleAnimation translateYDoubleAnimation = _storyboard.Children[3] as DoubleAnimation;
            translateYDoubleAnimation.From = translate.Y;
            translateYDoubleAnimation.To = Target.Position.Y;
            translateYDoubleAnimation.Duration = duration;
            //DoubleAnimation translateYDoubleAnimation = SetupDoubleAnimation(translate.Y, Target.Position.Y, animationTime);
            SetTargetProperty(translateYDoubleAnimation, BorderLayoutHost.RenderTransformProperty.Name + "." + TranslateTransform.YProperty.Name);

            //Storyboard storyboard = new Storyboard();
            //storyboard.Completed += new EventHandler(Storyboard_Completed);
            //storyboard.Children.Add(widthDoubleAnimation);
            //storyboard.Children.Add(heightDoubleAnimation);
            //storyboard.Children.Add(translateXDoubleAnimation);
            //storyboard.Children.Add(translateYDoubleAnimation);
            ScreenShotHost();
            //storyboard.Begin(this, true);
            //this.Dispatcher.BeginInvoke((Action)delegate
            //{
            //    //Console.WriteLine("Animation:\t{0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
            //    _storyboard.Begin(this, true);
            //});
            _storyboard.Begin(this, true);
        }

        /// <summary>
        /// Set target property
        /// </summary>
        /// <param name="doubleAnimation"></param>
        /// <param name="propertyPath"></param>
        private void SetTargetProperty(DoubleAnimation doubleAnimation, string propertyPath)
        {
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(propertyPath));
            Storyboard.SetTarget(doubleAnimation, this);
        }

        /// <summary>
        /// Create a double animation
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private DoubleAnimation SetupDoubleAnimation(double from, double to, int duration)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration));
        }

        private void ScreenShotHost()
        {
            if (AllowScreenShot)
                this.Container.imgIcon.Source = GetPNGImage(this.Container.grdContent, 1, 100);
        }

        /// <summary>
        /// Gets a PNG "screenshot" of the current UIElement
        /// </summary>
        /// <param name="source">UIElement to screenshot</param>
        /// <param name="scale">Scale to render the screenshot</param>
        /// <param name="quality">PNG Quality</param>
        /// <returns>Byte array of PNG data</returns>
        public static BitmapImage GetPNGImage(FrameworkElement source, double scale, int quality)
        {
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();

            // Save current canvas transform
            Transform transform = source.LayoutTransform;

            // Get size of control
            Size sizeOfControl = new Size(source.ActualWidth, source.ActualHeight);

            // Measure and arrange the control
            source.Measure(sizeOfControl);

            // Arrange the surface
            source.Arrange(new Rect(sizeOfControl));

            // Create and render surface and push bitmap to it
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)sizeOfControl.Width, (int)sizeOfControl.Height, 96d, 96d, PixelFormats.Pbgra32);

            // Now render surface to bitmap
            renderBitmap.Render(source);

            // Encode PNG data puch rendered bitmap into it
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            // Create BitmapImage from pngEncoder
            BitmapImage bitmapImage = new BitmapImage();
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                pngEncoder.Save(memoryStream);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return bitmapImage;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Match host when layout updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Target_LayoutUpdated(object sender, EventArgs e)
        {
            // Run animate when target layout updated
            Point to = Target.TranslatePoint(new Point(0, 0), Parent as UIElement);
            if (Target.ActualWidth != Target.Size.Width ||
                Target.ActualHeight != Target.Size.Height ||
                to.X != Target.Position.X || to.Y != Target.Position.Y &&
                IsAnimationCompleted)
                AnimationToTarget(_animationTime);
        }

        /// <summary>
        /// Run load animation when loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Target_Loaded(object sender, RoutedEventArgs e)
        {
            // Run animate for host when target is loaded
            if (IsAnimationCompleted)
            {
                AnimationToTarget(_animationTime);
                this.Target.LayoutUpdated += new EventHandler(Target_LayoutUpdated);
            }
        }

        /// <summary>
        /// Animation is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Storyboard_Completed(object sender, EventArgs e)
        {
            // Update the new size of host
            Width = Target.Size.Width;
            Height = Target.Size.Height;

            // Update the new position of host
            //Point to = Target.TranslatePoint(new Point(0, 0), Parent as UIElement);
            TranslateTransform translate = RenderTransform as TranslateTransform;
            translate.X = Target.Position.X;
            translate.Y = Target.Position.Y;

            if (IsRefreshData && ViewModelBase != null)
            {
                ViewModelBase.LoadData();
                IsRefreshData = false;
            }

            // Animation finish
            IsAnimationCompleted = true;

            // Turn off screenshot mode
            AllowScreenShot = false;
        }

        #endregion
    }
}
