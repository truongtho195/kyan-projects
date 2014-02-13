using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Resources;
using System.Windows.Interop;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace CPCToolkitExt.TextBoxControl
{
    public class TextBoxCustomBase : TextBox
    {
        #region Field
        public static string getImages = string.Empty;
        private ImageBrush textImageBrush;
        protected bool _formated = false;
        protected string _oldValue = string.Empty;
        protected Thickness BorderTextBoxBase;
        protected Brush BackgroundTextBoxBase;
        #endregion

        #region Contrustor
        public TextBoxCustomBase(string image)
        {
            this.AllowDrop = false;
            this.ContextMenu = null;
            this.IsUndoEnabled = false;
            this.AcceptsReturn = false;
            //this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            //this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            //this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
            if (image != string.Empty)
            {
                Assembly as1 = Assembly.GetExecutingAssembly();
                ResourceManager rm = new ResourceManager("CPCToolkitExt.TextBoxControl.Background", as1);
                if (image == "Ssn")
                {
                    textImageBrush = CreateBrushFromBitmap((System.Drawing.Bitmap)rm.GetObject("Ssn"));
                    textImageBrush.Stretch = Stretch.Fill;
                    textImageBrush.AlignmentX = AlignmentX.Left;
                    this.Background = textImageBrush;
                }

                if (image == "TaxID")
                {
                    textImageBrush = CreateBrushFromBitmap((System.Drawing.Bitmap)rm.GetObject("TaxID"));
                    textImageBrush.Stretch = Stretch.Fill;
                    textImageBrush.AlignmentX = AlignmentX.Left;
                    this.Background = textImageBrush;
                }

                if (image == "Identification")
                {
                    textImageBrush = CreateBrushFromBitmap((System.Drawing.Bitmap)rm.GetObject("Identification"));
                    textImageBrush.Stretch = Stretch.Fill;
                    textImageBrush.AlignmentX = AlignmentX.Left;
                    this.Background = textImageBrush;
                }

                if (image == "Phone")
                {
                    textImageBrush = CreateBrushFromBitmap((System.Drawing.Bitmap)rm.GetObject("Phone"));
                    textImageBrush.Stretch = Stretch.Fill;
                    textImageBrush.AlignmentX = AlignmentX.Left;
                    this.Background = textImageBrush;
                }

                if (image == "Fax")
                {
                    textImageBrush = CreateBrushFromBitmap((System.Drawing.Bitmap)rm.GetObject("Fax"));
                    textImageBrush.Stretch = Stretch.Fill;
                    textImageBrush.AlignmentX = AlignmentX.Left;
                    this.Background = textImageBrush;
                }

                if (image == "Zip")
                {
                    textImageBrush = CreateBrushFromBitmap((System.Drawing.Bitmap)rm.GetObject("Zip"));
                    textImageBrush.Stretch = Stretch.Fill;
                    textImageBrush.AlignmentX = AlignmentX.Left;
                    this.Background = textImageBrush;
                }

                if (image == "CardNumber")
                {
                    textImageBrush = CreateBrushFromBitmap((System.Drawing.Bitmap)rm.GetObject("CardNumber"));
                    textImageBrush.Stretch = Stretch.Fill;
                    textImageBrush.AlignmentX = AlignmentX.Left;
                    this.Background = textImageBrush;
                }
                // Use the brush to paint the button's background.
                this.Margin = new System.Windows.Thickness(0, 2, 0, 0);
            }
            base.Loaded += new RoutedEventHandler(TextBoxCustomBase_Loaded);
            base.TextChanged += new System.Windows.Controls.TextChangedEventHandler(TextBoxCustomBase_TextChanged);
        }

        public TextBoxCustomBase()
        {
            this.AllowDrop = false;
            this.ContextMenu = null;
            this.IsUndoEnabled = false;
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Insert, ModifierKeys.Shift)));
            base.Loaded += new RoutedEventHandler(TextBoxCustomBase_Loaded);
            base.TextChanged += new System.Windows.Controls.TextChangedEventHandler(TextBoxCustomBase_TextChanged);
        }
        #endregion

        #region DependencyProperty

        #region Value
        //
        // Summary:
        //     Gets or sets the text contents value return of the text box. This is a dependency property.
        //
        // Returns:
        //     A string containing the text contents of the text box. The default is an
        //     "".
        public static readonly DependencyProperty ValueDependencyProperty = DependencyProperty.Register(
        "Value",
        typeof(string),
        typeof(TextBoxCustomBase),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeText)));

        public string Value
        {
            get { return (string)GetValue(ValueDependencyProperty); }
            set { SetValue(ValueDependencyProperty, value); }
        }
        #endregion

        #region IsTextBlock

        public bool IsTextBlock
        {
            get { return (bool)GetValue(IsTextBlockProperty); }
            set { SetValue(IsTextBlockProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTextBlock.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextBlockProperty =
            DependencyProperty.Register("IsTextBlock", typeof(bool), typeof(TextBoxCustomBase),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsTextBlock)));

        protected static void ChangeIsTextBlock(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue != e.OldValue && bool.Parse(e.NewValue.ToString()))
                (source as TextBoxCustomBase).ChangeStyle();
            else
                (source as TextBoxCustomBase).PreviousStyle();
        }

        #endregion

        #region ChangeBackground

        public Brush ChangeBackground
        {
            get { return (Brush)GetValue(ChangeBackgroundProperty); }
            set { SetValue(ChangeBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChangeBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChangeBackgroundProperty =
            DependencyProperty.Register("ChangeBackground", typeof(Brush), typeof(TextBoxCustomBase), new UIPropertyMetadata(Brushes.White));


        #endregion

        #endregion

        #region Event

        protected static void ChangeText(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (source as TextBoxCustomBase).UpdateText(e.NewValue.ToString(), true);
            else
                (source as TextBoxCustomBase).ChangeBackGround();
        }

        public virtual void TextBoxCustomBase_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                //Set Background for textbox
                if (this.textImageBrush != null)
                    this.Background = textImageBrush;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        protected void TextBoxCustomBase_Loaded(object sender, RoutedEventArgs e)
        {
            ////Get BorderTextBoxBase , BackgroundTextBoxBase
            if (this.BackgroundTextBoxBase == null)
                this.BackgroundTextBoxBase = this.Background;
            if (this.BorderTextBoxBase == null || this.BorderTextBoxBase == new Thickness(0))
                this.BorderTextBoxBase = this.BorderThickness;
        }

        public virtual void UpdateText(string NewText, bool root)
        {

        }
        #endregion

        #region Methods

        ///checkNonSymbolstrange
        public bool CheckNonSymbolstrange(char localchar)
        {
            int temp = char.ConvertToUtf32(localchar.ToString(), 0);
            ////"." , 0-->9
            if (
                temp == 48 || // 0
                temp == 49 || // 1
                temp == 50 || // 2
                temp == 51 || // 3
                temp == 52 || // 4
                temp == 53 || // 5
                temp == 54 || // 6
                temp == 55 || // 7
                temp == 56 || // 8
                temp == 57    // 9
                )
                return true;

            return false;
        }

        public virtual void ChangeBackGround()
        {
            if (this.textImageBrush != null)
                this.Background = textImageBrush;
            this.Text = string.Empty;
        }

        private System.Windows.Media.ImageBrush CreateBrushFromBitmap(System.Drawing.Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return new System.Windows.Media.ImageBrush(bitmapSource);
        }

        public void ChangeStyle()
        {
            this.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Input,
                                    (ThreadStart)delegate
                                    {
                                        this.BorderThickness = new Thickness(0);
                                        this.Background = Brushes.Transparent;
                                        this.IsReadOnly = true;
                                    });
        }

        public void PreviousStyle()
        {
            this.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Input,
                                    (ThreadStart)delegate
                                    {
                                        this.BorderThickness = this.BorderTextBoxBase;
                                        this.Background = this.BackgroundTextBoxBase;
                                        this.IsReadOnly = false;
                                    });
        }
        #endregion
    }
}
