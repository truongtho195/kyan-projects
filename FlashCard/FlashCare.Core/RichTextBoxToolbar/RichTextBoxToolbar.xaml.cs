using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;


namespace RichTextBoxControl
{
    /// <summary>
    /// Interaction logic for RichTextBoxToolbar.xaml
    /// </summary>
    public partial class RichTextBoxToolbar : UserControl, INotifyPropertyChanged
    {
        // Static member variables
        private static ToggleButton m_SelectedAlignmentButton;
        private static ToggleButton m_SelectedListButton;

        protected bool IsDocumentChanged = false;

        //private int m_InternalUpdatePending;
        //private bool m_TextHasChanged;

        #region Destructor & Constructors
        public RichTextBoxToolbar()
        {
            InitializeComponent();
            this.Initialize();
            //Initialize UI
            this.BorderThickness = new Thickness(1, 1, 1, 1);
            //Events
            this.rtContent.TextChanged += new TextChangedEventHandler(rtContent_TextChanged);
            this.btnNormalText.Click += new RoutedEventHandler(btnNormalText_Click);
            this.btnCodeBlock.Click += new RoutedEventHandler(btnCodeBlock_Click);
            this.btnInlineCode.Click += new RoutedEventHandler(btnInlineCode_Click);
            this.btnBulletsButton.Click += new RoutedEventHandler(btnListsButton_Click);
            this.btnNumberingButton.Click += new RoutedEventHandler(btnListsButton_Click);
            this.rtContent.MouseRightButtonUp += new MouseButtonEventHandler(rtContent_MouseRightButtonUp);
            this.FontFamilyCombo.SelectionChanged += new SelectionChangedEventHandler(FontFamilyCombo_SelectionChanged);
            this.FontSizeCombo.SelectionChanged += new SelectionChangedEventHandler(FontSizeCombo_SelectionChanged);
            this.cbTextColor.SelectionChanged += new SelectionChangedEventHandler(cbTextColor_SelectionChanged);
            this.rtContent.SelectionChanged += new RoutedEventHandler(rtContent_SelectionChanged);
            this.rtContent.ContextMenu = null;
            rtContent.LostFocus += new RoutedEventHandler(rtContent_LostFocus);
        }

        void rtContent_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void rtContent_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SetToolbar();
        }
        #endregion

        #region Properties

        #region Visibility
        private Visibility _visibilityMenu;
        /// <summary>
        /// Gets or sets the Visibility.
        /// </summary>
        public Visibility VisibilityMenu
        {
            get { return _visibilityMenu; }
            set
            {
                if (_visibilityMenu != value)
                {
                    _visibilityMenu = value;
                    RaisePropertyChanged(() => Visibility);
                }
            }
        }
        #endregion

        #endregion

        #region Properties Dependency

        public FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(FlowDocument), typeof(RichTextBoxToolbar), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnDocumentChanged)));


        // CodeControlsVisibility property
        public static readonly DependencyProperty CodeControlsVisibilityProperty =
            DependencyProperty.Register("CodeControlsVisibility", typeof(Visibility),
            typeof(RichTextBoxToolbar));


        //// ToolbarBackground property
        //public Brush Foreground
        //{
        //    get { return (Brush)GetValue(ForegroundProperty); }
        //    set { SetValue(ForegroundProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ToolbarBackgrourd.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ForegroundProperty =
        //    DependencyProperty.Register("Foreground", typeof(Brush),
        //    typeof(RichTextBoxToolbar), new PropertyMetadata(SystemColors.MenuBrush));



        // ToolbarBackground property
        public Brush ToolbarBackground
        {
            get { return (Brush)GetValue(ToolbarBackgrourdProperty); }
            set { SetValue(ToolbarBackgrourdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToolbarBackgrourd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToolbarBackgrourdProperty =
            DependencyProperty.Register("ToolbarBackground", typeof(Brush),
            typeof(RichTextBoxToolbar), new PropertyMetadata(SystemColors.MenuBrush));

        // ToolbarBorderBrush property
        public Brush ToolbarBorderBrush
        {
            get { return (Brush)GetValue(ToolbarBorderBrushProperty); }
            set { SetValue(ToolbarBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ToolbarBorderBrushProperty =
            DependencyProperty.Register("ToolbarBorderBrush", typeof(Brush),
            typeof(RichTextBoxToolbar), new PropertyMetadata(SystemColors.MenuBarBrush));

        // ToolbarBorderThickness property
        [CategoryAttribute("Appearance")]
        public Thickness ToolbarBorderThickness
        {
            get { return (Thickness)GetValue(ToolbarBorderThicknessProperty); }
            set { SetValue(ToolbarBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty ToolbarBorderThicknessProperty =
            DependencyProperty.Register("ToolbarBorderThickness", typeof(Thickness),
            typeof(RichTextBoxToolbar), new PropertyMetadata());



        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RichTextBoxToolbar), new PropertyMetadata(false, new PropertyChangedCallback(OnIsReadOnlyChanged)));


        /// <summary>
        /// Called when the Document property is changed
        /// </summary>
        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (e.NewValue == null)
                (d as RichTextBoxToolbar).popup.Visibility = Visibility.Visible;
            else
                if (e.NewValue != e.OldValue)
                {
                    var t = bool.Parse(e.NewValue.ToString());
                    if (t)
                    {
                        (d as RichTextBoxToolbar).popup.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        (d as RichTextBoxToolbar).popup.Visibility = Visibility.Visible;
                    }
                }
        }

        #endregion

        #region PropertyChanged Callback Methods

        /// <summary>
        /// Called when the Document property is changed
        /// </summary>
        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //try
            //{
            //    RichTextBoxToolbar thisControl = (RichTextBoxToolbar)d;
            //    if (thisControl.IsDocumentChanged) return;
            //    thisControl.IsDocumentChanged = true;
            //    if (e.NewValue == null)
            //    {
            //        //Document is not amused by null :)
            //        thisControl.rtContent.Document = new FlowDocument();
            //        thisControl.IsDocumentChanged = false;
            //        return;
            //    }
            //    else if (e.NewValue != e.OldValue)
            //    {
            //        thisControl.rtContent.Document.Blocks.Clear();
            //        MemoryStream ms = new MemoryStream();
            //        XamlWriter.Save(e.NewValue as FlowDocument, ms);
            //        ms.Seek(0, SeekOrigin.Begin);

            //        thisControl.rtContent.Document = XamlReader.Load(ms) as FlowDocument;

            //    }
            //    thisControl.IsDocumentChanged = false;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<OnDocumentChanged>>>>>>>>>>>>>>>>>>" + ex.ToString());
            //}



            //ver 2
            try
            {
                // Initialize
                var thisControl = (RichTextBoxToolbar)d;

                if (thisControl.IsDocumentChanged) return;
                thisControl.IsDocumentChanged = true;

                // Set Document property on RichTextBox

                if (e.NewValue == null)
                {
                    //Document is not amused by null :)
                    thisControl.rtContent.Document = new FlowDocument();
                }
                else if (e.NewValue != e.OldValue)
                {
                    MemoryStream ms = new MemoryStream();
                    XamlWriter.Save(e.NewValue as FlowDocument, ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    thisControl.rtContent.Document = XamlReader.Load(ms) as FlowDocument;
                }


                thisControl.IsDocumentChanged = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("||=======================OnDocumentChanged=======================");
                Console.WriteLine("||{0}", ex.ToString());
            }
        }



        #endregion

        #region Events
        private void rtContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (this.IsDocumentChanged) return;
                this.IsDocumentChanged = true;
                //SetToolbar();

                //TextRange textRange = new TextRange(this.rtContent.Document.ContentStart, this.rtContent.Document.ContentEnd);
                //var test = GetImagesXML(this.rtContent.Document);
                //m_InternalUpdatePending = 2;
                this.Document = this.rtContent.Document;
                this.IsDocumentChanged = false;
            }
            catch (Exception ex)
            {
                Console.Write("<<<<<<<<<<<<RichTextBoxControl_TextChanged>>>>>>>>>>>>>>>>>" + ex.ToString());
            }

            //ver 2
            //try
            //{

            //    //m_TextHasChanged = true;
            //    if (this.IsDocumentChanged) return;
            //    this.IsDocumentChanged = true;


            //    // Set Document property
            //    SetValue(DocumentProperty, this.rtContent.Document);
            //    this.IsDocumentChanged = false;

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("||=======================rtContent_TextChanged=======================");
            //    Console.WriteLine("||{0}", e.ToString());
            //}

        }




        private void btnNormalText_Click(object sender, RoutedEventArgs e)
        {
            var textRange = new TextRange(rtContent.Selection.Start, rtContent.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, FontFamily);
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, FontSize);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Foreground);
            textRange.ApplyPropertyValue(Block.MarginProperty, new Thickness(Double.NaN));
        }

        private void btnListsButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = (ToggleButton)sender;
            var buttonGroup = new[] { btnBulletsButton, btnNumberingButton };
            this.SetButtonGroupSelection(clickedButton, m_SelectedListButton, buttonGroup, false);
            m_SelectedListButton = clickedButton;
        }

        private void OnAlignmentButtonClick(object sender, RoutedEventArgs e)
        {
            var clickedButton = (ToggleButton)sender;
            var buttonGroup = new[] { btnLeftButton, btnCenterButton, btnRightButton, JustifyButton };
            this.SetButtonGroupSelection(clickedButton, m_SelectedAlignmentButton, buttonGroup, true);
            m_SelectedAlignmentButton = clickedButton;
        }

        private void btnInlineCode_Click(object sender, RoutedEventArgs e)
        {
            var textRange = new TextRange(rtContent.Selection.Start, rtContent.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, "Consolas");
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, 11D);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, "FireBrick");
        }

        private void btnCodeBlock_Click(object sender, RoutedEventArgs e)
        {
            var textRange = new TextRange(rtContent.Selection.Start, rtContent.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, "Consolas");
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, "FireBrick");
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, 11D);
            textRange.ApplyPropertyValue(Block.MarginProperty, new Thickness(0));
        }

        private void cbTextColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Exit if no selection
            if (cbTextColor.SelectedItem == null) return;
            // clear selection if value unset
            if (cbTextColor.SelectedItem.ToString() == "{DependencyProperty.UnsetValue}")
            {
                cbTextColor.SelectedItem = null;
                return;
            }

            var textRange = new TextRange(rtContent.Selection.Start, rtContent.Selection.End);
            //System.Windows.Media.SolidColorBrush
            var color = cbTextColor.SelectedValue.ToString().Replace("System.Windows.Media.SolidColorBrush", "");
            var t = (Color)ColorConverter.ConvertFromString(color);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(t));
        }

        private void FontSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Exit if no selection
            if (FontSizeCombo.SelectedItem == null) return;

            // clear selection if value unset
            if (FontSizeCombo.SelectedItem.ToString() == "{DependencyProperty.UnsetValue}")
            {
                FontSizeCombo.SelectedItem = null;
                return;
            }

            // Process selection
            var pointSize = FontSizeCombo.SelectedItem.ToString();
            var pixelSize = Convert.ToDouble(pointSize) * (96 / 72);
            var textRange = new TextRange(rtContent.Selection.Start, rtContent.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, pixelSize);
        }

        private void FontFamilyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontFamilyCombo.SelectedItem == null) return;
            var fontFamily = FontFamilyCombo.SelectedItem.ToString();
            var textRange = new TextRange(rtContent.Selection.Start, rtContent.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
        }

        private void rtContent_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsReadOnly)
            {
                this.popup.IsOpen = true;
                this.popup.StaysOpen = false;
            }
        }

        private void rtContent_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.popup.IsOpen = true;
            this.popup.StaysOpen = false;
        }

        private void rtContent_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.popup.StaysOpen = false;
            this.popup.IsOpen = true;
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            FontFamilyCombo.ItemsSource = Fonts.SystemFontFamilies;
            FontSizeCombo.Items.Add("10");
            FontSizeCombo.Items.Add("11");
            FontSizeCombo.Items.Add("12");
            FontSizeCombo.Items.Add("13");
            FontSizeCombo.Items.Add("14");
            FontSizeCombo.Items.Add("15");
            FontSizeCombo.Items.Add("16");
            FontSizeCombo.Items.Add("17");
            FontSizeCombo.Items.Add("18");
            FontSizeCombo.Items.Add("20");
            FontSizeCombo.Items.Add("22");
            FontSizeCombo.Items.Add("24");
            FontSizeCombo.Items.Add("26");
            FontSizeCombo.Items.Add("28");

            BrushConverter converter = new BrushConverter();
            //var  colors = (from p in typeof(Brushes).GetProperties()
            //            select ((Brush)converter.ConvertFromString(p.Name)).Clone()).ToList();
            cbTextColor.ItemsSource = typeof(Brushes).GetProperties();
        }
        private void SetButtonGroupSelection(ToggleButton clickedButton, ToggleButton currentSelectedButton, IEnumerable<ToggleButton> buttonGroup, bool ignoreClickWhenSelected)
        {
            /* In some cases, if the user clicks the currently-selected button, we want to ignore
             * the click; for example, when a text alignment button is clicked. In other cases, we
             * want to deselect the button, but do nothing else; for example, when a list butteting
             * or numbering button is clicked. The ignoreClickWhenSelected variable controls that
             * behavior. */

            // Exit if currently-selected button is clicked
            if (clickedButton == currentSelectedButton)
            {
                if (ignoreClickWhenSelected) clickedButton.IsChecked = true;
                return;
            }

            // Deselect all buttons
            foreach (var button in buttonGroup)
            {
                button.IsChecked = false;
            }

            // Select the clicked button
            clickedButton.IsChecked = true;
        }

        private void SetToolbar()
        {
            // Set font family combo
            var textRange = new TextRange(rtContent.Selection.Start, rtContent.Selection.End);
            var fontFamily = textRange.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamilyCombo.SelectedItem = fontFamily;

            // Set font size combo
            var fontSize = textRange.GetPropertyValue(TextElement.FontSizeProperty);
            FontSizeCombo.Text = fontSize.ToString();
            // Set Font Color
            var foregroundColor = textRange.GetPropertyValue(TextElement.ForegroundProperty);
            BrushConverter conv = new BrushConverter();
            if (!"{DependencyProperty.UnsetValue}".Equals(foregroundColor.ToString()))
            {
                SolidColorBrush color = conv.ConvertFromString(foregroundColor.ToString()) as SolidColorBrush;
                var colorInfo = this.cbTextColor.ItemsSource.Cast<PropertyInfo>();

                this.cbTextColor.SelectedItem = colorInfo.Count() > 1 ? null : colorInfo.SingleOrDefault(x => (new BrushConverter().ConvertFromString(x.Name) as SolidColorBrush).Color == color.Color);
            }
            else
                this.cbTextColor.SelectedItem = null;
            // Set Font buttons
            if (!String.IsNullOrEmpty(textRange.Text))
            {
                BoldButton.IsChecked = textRange.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
                ItalicButton.IsChecked = textRange.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
                UnderlineButton.IsChecked = textRange.GetPropertyValue(Inline.TextDecorationsProperty).Equals(TextDecorations.Underline);
            }

            // Set Alignment buttons
            btnLeftButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
            btnCenterButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
            btnRightButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);
            JustifyButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Justify);
        }



        private string GetImagesXML(FlowDocument flowDocument)
        {
            using (StringWriter stringwriter = new StringWriter())
            {
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stringwriter))
                {

                    Type inlineType;
                    InlineUIContainer uic;
                    System.Windows.Controls.Image replacementImage;
                    byte[] bytes;
                    System.Text.ASCIIEncoding enc;

                    //loop through replacing images in the flowdoc with the byte versions
                    foreach (Block b in flowDocument.Blocks)
                    {
                        foreach (Inline i in ((Paragraph)b).Inlines)
                        {
                            inlineType = i.GetType();

                            if (inlineType == typeof(Run))
                            {
                                //The inline is TEXT!!!
                            }
                            else if (inlineType == typeof(InlineUIContainer))
                            {
                                //The inline has an object, likely an IMAGE!!!
                                uic = ((InlineUIContainer)i);

                                //if it is an image
                                if (uic.Child.GetType() == typeof(System.Windows.Controls.Image))
                                {
                                    //grab the image
                                    replacementImage = (System.Windows.Controls.Image)uic.Child;

                                    //get its byte array
                                    bytes = GetImageByteArray((System.Windows.Media.Imaging.BitmapImage)replacementImage.Source);
                                    //write the element
                                    writer.WriteStartElement("Image");
                                    //put the bytes into the tag
                                    enc = new System.Text.ASCIIEncoding();
                                    writer.WriteString(enc.GetString(bytes));
                                    //close the element
                                    writer.WriteEndElement();
                                }
                            }
                        }
                    }
                }

                return stringwriter.ToString();
            }
        }
        //This function is where the problem is, i need a way to get the byte array
        private byte[] GetImageByteArray(System.Windows.Media.Imaging.BitmapImage bi)
        {
            byte[] result = new byte[0];
            using (MemoryStream ms = new MemoryStream())
            {
                XamlWriter.Save(bi, ms);
                //result = new byte[ms.Length];
                result = ms.ToArray();
            }
            return result;
        }
        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

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

        private void tgbtnColor_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            cbTextColor.IsDropDownOpen = false;
        }



        #endregion

    }
}
