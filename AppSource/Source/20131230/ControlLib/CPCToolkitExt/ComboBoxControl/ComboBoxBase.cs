using System;
using System.Collections.Generic;
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
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using CPCToolkitExtLibraries;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Reflection;

namespace CPCToolkitExt.ComboBoxControl
{
    public class ComboBoxBase : UserControl, INotifyPropertyChanged
    {
        #region Constructors

        #endregion

        #region Field
        //Get the BorderThickness of Control
        protected Thickness BorderBase;
        //Get the Background of Control
        protected Brush BackgroundBase;
        //Set reload data of control
        protected bool IsLoad = false;
        //Set reload the contents of TextBox
        protected bool IsSelectedItem = false;
        protected bool IsNavigation = false;
        protected object SelectedItemClone = null;
        #endregion

        #region Properties

        #region ISOpenPopup
        /// <summary>
        ///  Get the IsOpen of the popup.
        /// </summary>
        public bool ISOpenPopup
        {
            get;
            set;
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

        #region DependencyProperties

        #region InputMaxLength
        public int InputMaxLength
        {
            get { return (int)GetValue(InputMaxLengthProperty); }
            set { SetValue(InputMaxLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputMaxLenght.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputMaxLengthProperty =
            DependencyProperty.Register("InputMaxLength", typeof(int), typeof(ComboBoxBase), new UIPropertyMetadata(0));

        #endregion

        #region IsTextBlock
        /// <summary>
        /// Set control is the TextBlock.
        /// </summary>
        public bool IsTextBlock
        {
            get { return (bool)GetValue(IsTextBlockProperty); }
            set { SetValue(IsTextBlockProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTextBlock.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextBlockProperty =
            DependencyProperty.Register("IsTextBlock", typeof(bool), typeof(ComboBoxBase),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsTextBlock)));

        protected static void ChangeIsTextBlock(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null
                && e.NewValue != e.OldValue
                && bool.Parse(e.NewValue.ToString()))
                (source as ComboBoxBase).ChangeStyle();
            else
                (source as ComboBoxBase).PreviousStyle();
        }

        #endregion

        #region IsReadOnly
        //
        // Summary:
        //     Gets or sets a value that indicates whether the text editing control is read-only
        //     to a user interacting with the control. This is a dependency property.
        //
        // Returns:
        //     true if the contents of the text editing control are read-only to a user;
        //     otherwise, the contents of the text editing control can be modified by the
        //     user. The default value is false.

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ComboBoxBase),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(ChangeIsReadOnly)));

        protected static void ChangeIsReadOnly(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null
                && e.NewValue != e.OldValue
                && bool.Parse(e.NewValue.ToString()))
                (source as ComboBoxBase).ChangeReadOnly(true);
            else
                (source as ComboBoxBase).ChangeReadOnly(false);
        }

        #endregion

        #region NameChildrenControl
        public object NameChildrenControl
        {
            get { return (object)GetValue(NameChildrenControlProperty); }
            set { SetValue(NameChildrenControlProperty, value); }
        }
        // Using a DependencyProperty as the backing store for NameChildrenControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameChildrenControlProperty =
            DependencyProperty.Register("NameChildrenControl", typeof(object), typeof(ComboBoxBase), new UIPropertyMetadata(null));
        #endregion

        #region ContentAlignment
        //
        // Summary:
        //     Gets or sets the horizontal alignment of the control's content.
        //
        // Returns:
        //     One of the System.Windows.HorizontalAlignment values. The default is System.Windows.HorizontalAlignment.Left.
        public HorizontalAlignment ContentAlignment
        {
            get { return (HorizontalAlignment)GetValue(ContentAlignmentProperty); }
            set { SetValue(ContentAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentAlignmentProperty =
            DependencyProperty.Register("ContentAlignment", typeof(HorizontalAlignment), typeof(ComboBoxBase), new UIPropertyMetadata(HorizontalAlignment.Right));

        #endregion

        #region ButtonBackground
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of Button Background
        //
        [Category("Brushes")]
        public Brush ButtonBackground
        {
            get { return (Brush)GetValue(ButtonBackgroundProperty); }
            set { SetValue(ButtonBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register("ButtonBackground", typeof(Brush), typeof(ComboBoxBase), new UIPropertyMetadata(Brushes.Brown));

        #endregion

        #region RegularPolygonBackground
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of RegularPolygon BackgroundProperty
        //
        [Category("Brushes")]
        public Brush RegularPolygonBackground
        {
            get { return (Brush)GetValue(RegularPolygonBackgroundProperty); }
            set { SetValue(RegularPolygonBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RegularPolygonBackgroundProperty =
            DependencyProperty.Register("RegularPolygonBackground", typeof(Brush), typeof(ComboBoxBase), new UIPropertyMetadata(Brushes.White));

        #endregion

        #region SelectedItemBackground
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of item.
        //
        [Category("Brushes")]
        public Brush SelectedItemBackground
        {
            get { return (Brush)GetValue(SelectedItemBackgroundProperty); }
            set { SetValue(SelectedItemBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            DependencyProperty.Register("SelectedItemBackground", typeof(Brush), typeof(ComboBoxBase), new UIPropertyMetadata(Brushes.Blue));

        #endregion

        #region MoveOverItemBrush
        //
        // Summary:
        //     Gets or sets a value that indicates whether a user can change value of item.
        //
        [Category("Brushes")]
        public Brush MoveOverItemBrushBackground
        {
            get { return (Brush)GetValue(MoveOverItemBrushProperty); }
            set { SetValue(MoveOverItemBrushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AutoChangeSelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveOverItemBrushProperty =
            DependencyProperty.Register("MoveOverItemBrush", typeof(Brush), typeof(ComboBoxBase), new UIPropertyMetadata(Brushes.Yellow));

        #endregion

        #endregion

        #region Methods

        #region ChangeStyle
        protected virtual void ChangeStyle()
        {
            //Set Background ,BorderThickness when IsReadOnly= true
            this.Dispatcher.BeginInvoke(
                            DispatcherPriority.Input,
                            (ThreadStart)delegate
                            {
                                this.Background = Brushes.Transparent;
                                this.BorderThickness = new Thickness(0);
                            });

        }
        #endregion

        #region PreviousStyle
        protected virtual void PreviousStyle()
        {
            //Return value for Background,BorderThickness when IsReadOnly= false 
            this.Dispatcher.BeginInvoke(
                            DispatcherPriority.Input,
                            (ThreadStart)delegate
                            {
                                this.Background = this.BackgroundBase;
                                this.BorderThickness = this.BorderBase;
                            });
        }
        #endregion

        #region ChangeReadOnly
        protected virtual void ChangeReadOnly(bool isReadOnly)
        {

        }
        #endregion

        #region IsCancelKey
        protected bool IsCancelKey(Key key)
        {
            return key == Key.Escape || key == Key.Enter || key == Key.Tab;
        }
        #endregion

        #region IsNavigationKey
        protected bool IsNavigationKey(Key Pressed)
        {

            return Pressed == Key.Up
                || Pressed == Key.Down
                || Pressed == Key.PageUp
               || Pressed == Key.PageDown;
        }
        #endregion

        #region Check value
        protected bool IsNumeric(string input)
        {
            Regex pattern = new Regex("[.0-9]");
            return pattern.IsMatch(input);
        }

        protected bool IsCharacter(string input)
        {
            Regex pattern = new Regex("[a-z A-Z ]");
            return pattern.IsMatch(input);
        }

        #endregion

        #endregion
    }
}
