using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPCToolkitExt.ComboBoxControl
{
    public class ComboBoxControl : ComboBox, INotifyPropertyChanged
    {
        #region Fields

        //To save value of BtnNew_Path on style.
        private Button _btnNew;

        //To Check character is repeat when input in ComboBox
        private int _repeat;

        //To Use store previous character when input in ComboBox
        private string _previousChar;

        #endregion

        #region Constructors

        #endregion

        #region Methods

        //
        // Summary:
        //     Builds the current template's visual tree if necessary, and returns a value
        //     that indicates whether the visual tree was rebuilt by this call.
        //
        // Returns:
        //     true if visuals were added to the tree; returns false otherwise.
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            try
            {
                _btnNew = GetTemplateChild("BtnNew_Path") as Button;
                if (this._btnNew != null)
                    _btnNew.Click += new RoutedEventHandler(Button_Click);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(" ***** ComboBoxControl OnApplyTemplate ****" + ex.ToString());
            }

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the HiddenMemberPath
        /// </summary>
        public string HiddenMemberPath { get; set; }

        #region AutoHiddenItem
        /// <summary>
        /// To get , set value when user select an item.
        /// </summary>
        private bool _autoHiddenItem = true;
        public bool AutoHiddenItem
        {
            get { return _autoHiddenItem; }
            set
            {
                if (_autoHiddenItem != value)
                {
                    _autoHiddenItem = value;
                    RaisePropertyChanged(() => AutoHiddenItem);
                }
            }
        }
        #endregion

        #region AutoVisibleItem
        /// <summary>
        /// To get , set value when user select an item.
        /// </summary>
        private bool _autoVisibleItem = true;
        public bool AutoVisibleItem
        {
            get { return _autoVisibleItem; }
            set
            {
                if (_autoVisibleItem != value)
                {
                    _autoVisibleItem = value;
                    RaisePropertyChanged(() => AutoVisibleItem);
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

        #region Events

        /// <summary>
        /// It is event of button.it will run when users click button " Click to add item."
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.AddItemCommand != null)
                this.AddItemCommand.Execute(this.AddItemCommandParameter);
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Update property to hide item
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (this.IsLoaded && this.AutoHiddenItem)
            {
                // Set value to hidden selec+ted item
                foreach (object newItem in e.AddedItems)
                    // Don't hidden item that selected value equal 0
                    if (!newItem.GetType().GetProperty(this.SelectedValuePath).GetValue(newItem, null).ToString().Equals("0"))
                        newItem.GetType().GetProperty(this.HiddenMemberPath).SetValue(newItem, true, null);

                // Set value to visible other items
                if (this.AutoVisibleItem)
                    foreach (object oldItem in e.RemovedItems)
                        oldItem.GetType().GetProperty(this.HiddenMemberPath).SetValue(oldItem, false, null);
            }
        }

        /// <summary>
        /// Process hide item when press key
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (this.AutoHiddenItem)
            {
                // Cast items source to object list
                List<object> itemList = this.ItemsSource.Cast<object>().ToList();

                // Get visible items list and selected item
                List<object> visibleItemList = itemList.Where(
                    x => !(bool)x.GetType().GetProperty(this.HiddenMemberPath).GetValue(x, null) ||
                    x.Equals(this.SelectedItem)).ToList();

                // Get visible items list and selected item
                List<object> exceptList = visibleItemList.Where(
                    x => !x.Equals(this.SelectedItem)).ToList();

                // Get current index
                int currentIndex = this.SelectedIndex;

                bool isHidden;
                object newItem;

                switch (e.Key)
                {
                    case Key.Down:
                        if (exceptList.Count() > 0)
                        {
                            do
                            {
                                if (currentIndex == itemList.Count() - 1)
                                    currentIndex = -1;
                                newItem = itemList.ElementAt(++currentIndex);
                                isHidden = (bool)newItem.GetType().GetProperty(this.HiddenMemberPath).GetValue(newItem, null);
                            } while (isHidden);
                            this.SelectedIndex = currentIndex;
                        }
                        e.Handled = true;
                        break;
                    case Key.Up:
                        if (exceptList.Count() > 0)
                        {
                            do
                            {
                                if (currentIndex == 0)
                                    currentIndex = itemList.Count();
                                newItem = itemList.ElementAt(--currentIndex);
                                isHidden = (bool)newItem.GetType().GetProperty(this.HiddenMemberPath).GetValue(newItem, null);
                            } while (isHidden);
                            this.SelectedIndex = currentIndex;
                        }
                        e.Handled = true;
                        break;
                    case Key.Home:
                        if (exceptList.Count() > 0)
                        {
                            currentIndex = -1;
                            do
                            {
                                newItem = itemList.ElementAt(++currentIndex);
                                isHidden = (bool)newItem.GetType().GetProperty(this.HiddenMemberPath).GetValue(newItem, null);
                            } while (isHidden && currentIndex < this.SelectedIndex);
                            this.SelectedIndex = currentIndex;
                        }
                        e.Handled = true;
                        break;
                    case Key.End:
                        if (exceptList.Count() > 0)
                        {
                            currentIndex = itemList.Count();
                            do
                            {
                                newItem = itemList.ElementAt(--currentIndex);
                                isHidden = (bool)newItem.GetType().GetProperty(this.HiddenMemberPath).GetValue(newItem, null);
                            } while (isHidden && currentIndex > this.SelectedIndex);
                            this.SelectedIndex = currentIndex;
                        }
                        e.Handled = true;
                        break;
                    default:
                        string currentChar = e.Key.ToString();
                        if (currentChar.StartsWith("D"))
                            currentChar = currentChar.Substring(1);
                        else if (currentChar.StartsWith("NumPad"))
                            currentChar = currentChar.Substring(6);
                        if (System.Text.RegularExpressions.Regex.IsMatch(currentChar, @"^[A-Za-z0-9]$"))
                        {
                            // Get item list that name start with press key
                            List<object> resultList = visibleItemList.Where(
                                x => !x.GetType().GetProperty(this.SelectedValuePath).GetValue(x, null).ToString().Equals("0") &&
                                    x.GetType().GetProperty(this.DisplayMemberPath).GetValue(x, null).ToString().StartsWith(currentChar)).ToList();

                            if (resultList.Count > 0)
                            {
                                int index;
                                if (resultList.Count > 1)
                                {
                                    if (_previousChar != null)
                                    {
                                        if (++_repeat == resultList.Count)
                                            _repeat = 0;
                                        index = _repeat;
                                    }
                                    else
                                    {
                                        _previousChar = currentChar;
                                        _repeat = 0;
                                        index = 0;
                                    }
                                }
                                else
                                {
                                    index = 0;
                                    _previousChar = null;
                                }

                                // Set selected index for ComboBox
                                this.SelectedIndex = itemList.IndexOf(resultList.ElementAt(index));
                            }
                            else
                            {
                                _previousChar = null;
                                _repeat = -1;
                            }
                            e.Handled = true;
                        }
                        break;
                }
            }
        }

        #endregion

        #region DependencyProperties

        #region AddItemCommand

        //
        // Summary:
        //     Gets or sets the command to invoke when user click button to add a new item. This is a
        //     dependency property.
        //
        // Returns:
        //     A command to invoke when user click button to add a new item. The default value is null.
        public static readonly DependencyProperty AddItemCommandProperty =
            DependencyProperty.Register("AddItemCommand", typeof(ICommand), typeof(ComboBoxControl));

        [Category("Common Properties")]
        public ICommand AddItemCommand
        {
            get { return (ICommand)GetValue(AddItemCommandProperty); }
            set { SetValue(AddItemCommandProperty, value); }
        }

        #endregion

        #region AddItemCommandParameter

        //
        // Summary:
        //     Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property. This is a dependency property.
        //
        // Returns:
        //     Parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property.
        public static readonly DependencyProperty AddItemCommandParameterProperty =
            DependencyProperty.Register("AddItemCommandParameter", typeof(object), typeof(ComboBoxControl));

        [Category("Common Properties")]
        public object AddItemCommandParameter
        {
            get { return (object)GetValue(AddItemCommandParameterProperty); }
            set { SetValue(AddItemCommandParameterProperty, value); }
        }

        #endregion

        #region VisibilityAddItem
        /// <summary>
        /// To get , set value when add button is visible.
        /// </summary>
        public Visibility VisibilityAddItem
        {
            get { return (Visibility)GetValue(VisibilityAddItemProperty); }
            set { SetValue(VisibilityAddItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VisibilityAddItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityAddItemProperty =
            DependencyProperty.Register("VisibilityAddItem", typeof(Visibility), typeof(ComboBoxControl), new UIPropertyMetadata(Visibility.Visible)); 

        #endregion

        #endregion
    }
}
