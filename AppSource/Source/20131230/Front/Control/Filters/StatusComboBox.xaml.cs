using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Markup;
using System.Diagnostics;

namespace CPC.Control
{
    public delegate void CheckChangedEventHandler(StatusModel sender);

    public class StatusComboBox : UserControl, INotifyPropertyChanged, IComponentConnector
    {

        #region Members
        private bool _contentLoaded;
        private string _resultText;
        internal ComboBox mainComboBox;
        internal StatusComboBox statusComboBox;
        #endregion

        #region Dependency
        public static readonly DependencyProperty CheckAllTextProperty = DependencyProperty.Register("CheckAllText", typeof(string), typeof(StatusComboBox), new PropertyMetadata("All"));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList<StatusModel>), typeof(StatusComboBox), new PropertyMetadata(null, new PropertyChangedCallback(StatusComboBox.ItemsSourcePropertyChangedCallBack)));
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(List<StatusModel>), typeof(StatusComboBox));
        public static readonly DependencyProperty SeparatorProperty = DependencyProperty.Register("Separator", typeof(string), typeof(StatusComboBox), new PropertyMetadata(","));
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SelectionChanged;
        #endregion

        #region Methods
        public StatusComboBox()
        {
            this.InitializeComponent();
        }

        private static void allItem_CheckedChanged(StatusModel sender)
        {
            sender.ControlHolder.CheckAllItems(sender);
        }

        internal void CheckAllItems(StatusModel sender)
        {
            foreach (StatusModel base2 in this.ItemsSource)
            {
                if (!base2.IsAllItem)
                {
                    base2.IsAuto = true;
                    base2.IsChecked = sender.IsChecked;
                }
            }
            this.ItemsSource[1].OnCheckedChanged();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/Control/Filters/StatusComboBox.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        private static void item_CheckedChanged(StatusModel sender)
        {
            sender.ControlHolder.UpdateResultText(sender);
        }

        private static void ItemsSourcePropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            StatusComboBox box = sender as StatusComboBox;
            if (box != null)
            {
                IList<StatusModel> newValue = e.NewValue as IList<StatusModel>;
                if ((newValue != null) && (newValue.Count > 0))
                {
                    bool flag = false;
                    foreach (StatusModel base2 in newValue)
                    {
                        base2.ControlHolder = box;
                        base2.CheckedChanged += new CheckChangedEventHandler(StatusComboBox.item_CheckedChanged);
                        if (base2.IsAllItem)
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        StatusModel base4 = new StatusModel
                        {
                            IsAllItem = true,
                            Content = box.CheckAllText,
                            ControlHolder = box
                        };
                        StatusModel item = base4;
                        item.CheckedChanged += new CheckChangedEventHandler(StatusComboBox.allItem_CheckedChanged);
                        newValue.Insert(0, item);
                    }
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.statusComboBox = (StatusComboBox)target;
                    return;

                case 2:
                    this.mainComboBox = (ComboBox)target;
                    return;
            }
            this._contentLoaded = true;
        }

        internal void UpdateResultText(StatusModel sender)
        {
            List<StatusModel> list = new List<StatusModel>();
            this._resultText = null;
            bool flag = true;
            foreach (StatusModel base2 in this.ItemsSource)
            {
                if (base2.IsChecked && !base2.IsAllItem)
                {
                    this._resultText = this._resultText + (flag ? base2.Content : (this.Separator + " " + base2.Content));
                    list.Add(base2);
                    if (flag)
                    {
                        flag = false;
                    }
                }
            }
            if (list != null && list.Count > 0)
            {
                this.SelectedItems = list;
            }
            else
            {
                this.SelectedItems = null;
            }

            this.mainComboBox.SelectedIndex = this.ItemsSource.IndexOf(sender);
            this.OnPropertyChanged("Text");
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
            }
            if (!sender.IsAuto)
            {
                int num = this.ItemsSource.Count - 1;
                bool flag2 = list.Count == num;
                if (flag2 != this.ItemsSource[0].IsChecked)
                {
                    this.ItemsSource[0].IsAuto = true;
                    this.ItemsSource[0].IsChecked = flag2;
                }
            }
        }
        #endregion

        #region Properties
        public string CheckAllText
        {
            get
            {
                return (base.GetValue(CheckAllTextProperty) as string);
            }
            set
            {
                base.SetValue(CheckAllTextProperty, value);
            }
        }

        public IList<StatusModel> ItemsSource
        {
            get
            {
                return (base.GetValue(ItemsSourceProperty) as IList<StatusModel>);
            }
            set
            {
                base.SetValue(ItemsSourceProperty, value);
            }
        }

        public List<StatusModel> SelectedItems
        {
            get
            {
                return (base.GetValue(SelectedItemsProperty) as List<StatusModel>);
            }
            set
            {
                base.SetValue(SelectedItemsProperty, value);
            }
        }

        public string Separator
        {
            get
            {
                return (base.GetValue(SeparatorProperty) as string);
            }
            set
            {
                base.SetValue(SeparatorProperty, value);
            }
        }

        public string Text
        {
            get
            {
                return this._resultText;
            }
        }
        #endregion

    }

    public class StatusModel : INotifyPropertyChanged
    {

        #region Members & Variables
        private bool _isChecked;
        #endregion

        #region Events
        public event CheckChangedEventHandler CheckedChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        public void OnCheckedChanged()
        {
            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(this);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Properties
        public string Content { get; set; }

        public StatusComboBox ControlHolder { get; set; }

        public bool IsAllItem { get; internal set; }

        public bool IsAuto { get; set; }

        public bool IsChecked
        {
            get
            {
                return this._isChecked;
            }
            set
            {
                if (this._isChecked != value)
                {
                    this._isChecked = value;
                    this.OnPropertyChanged("IsChecked");
                    if (!this.IsAuto)
                    {
                        this.OnCheckedChanged();
                    }
                    else
                    {
                        this.IsAuto = false;
                    }
                }
            }
        }

        public int Key { get; set; }
        #endregion

    }
}