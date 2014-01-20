using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPC.Control
{
    public class ComboBoxHideItem : ComboBox
    {
        #region Defines

        // Check character is repeat when input in ComboBox
        private int _repeat;

        // Use store previous character when input in ComboBox
        private string _previousChar;

        #endregion

        #region Properties

        public string HiddenMemberPath { get; set; }

        #endregion

        #region Constructors

        public ComboBoxHideItem()
        {
            // Register preview key down event for ComboBox
            this.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(OnPreviewKeyDown);
        }

        #endregion

        #region Override Methods

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            // Set value to hidden selected item
            foreach (object newItem in e.AddedItems)
                // Don't hidden item that selected value equal 0
                if (!newItem.GetType().GetProperty(this.SelectedValuePath).GetValue(newItem, null).ToString().Equals("0"))
                    newItem.GetType().GetProperty(this.HiddenMemberPath).SetValue(newItem, true, null);

            // Set value to visible other items
            foreach (object oldItem in e.RemovedItems)
                oldItem.GetType().GetProperty(this.HiddenMemberPath).SetValue(oldItem, false, null);

        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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

        #endregion
    }
}
