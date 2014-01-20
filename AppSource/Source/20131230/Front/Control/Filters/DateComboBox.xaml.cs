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
using System.ComponentModel;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for DateComboBox.xaml
    /// </summary>
    public partial class DateComboBox : UserControl, INotifyPropertyChanged
    {
        #region Command

        // Command

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(DateComboBox), new UIPropertyMetadata(null));




        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(DateComboBox), new PropertyMetadata(null));



        //public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(DateComboBox), new PropertyMetadata(null));

        #endregion

        #region Constructors

        public DateComboBox()
        {
            InitializeComponent();
            this.cmbSearchContentOrderDate.SelectionChanged += new SelectionChangedEventHandler(cmbSearch_SelectionChanged);
            this.Loaded += new RoutedEventHandler(DateComboBox_Loaded);
            this.Today = DateTimeExt.Today;
        }

        #endregion

        #region Events

        private void DateComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.cmbSearchContentOrderDate.Items);

            collectionView.Filter = o =>
            {
                ValueDateSearch? value;
                Separator separator = o as Separator;
                if (separator != null)
                {
                    value = separator.Tag as ValueDateSearch?;

                    if (value.HasValue)
                    {
                        if (!_showAllDate && value.Equals(ValueDateSearch.All)) return false;
                        else if (!_showThisDate && value.Equals(ValueDateSearch.Today)) return false;
                        else if (!_showLastDate && value.Equals(ValueDateSearch.Yesterday)) return false;
                        else if (!_showNextDate && value.Equals(ValueDateSearch.Tomorrow)) return false;
                    }

                    return true;
                }

                ComboBoxItem item = o as ComboBoxItem;
                if (item == null) return true;

                value = item.Tag as ValueDateSearch?;

                if (value.HasValue)
                {
                    if (!_showAllDate && value.Equals(ValueDateSearch.All)) return false;
                    else
                    {
                        string name = Enum.GetName(typeof(ValueDateSearch), value);
                        if (!_showThisDate
                            && (value.Equals(ValueDateSearch.Today) || name.StartsWith("This"))) return false;
                        else if (!_showLastDate
                            && (value.Equals(ValueDateSearch.Yesterday) || name.StartsWith("Last"))) return false;
                        else if (!_showNextDate
                            && (value.Equals(ValueDateSearch.Tomorrow) || name.StartsWith("Next"))) return false;
                    }
                }
                return true;
            };
        }

        private void cmbSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSearchContentOrderDate.SelectedItem != null)
            {
                string text = (cmbSearchContentOrderDate.SelectedItem as ComboBoxItem).Content.ToString();

                if ((cmbSearchContentOrderDate.SelectedItem as Button) != null)
                {
                    cmbSearchContentOrderDate.SelectedItem = null;
                    FromDate = null;
                    ToDate = null;
                    return;
                }
                else
                {
                    if (Today.Year <= 1000)
                        throw new InvalidOperationException();

                    ValueDateSearch? value = (cmbSearchContentOrderDate.SelectedItem as ComboBoxItem).Tag as ValueDateSearch?; // Int32.Parse((cmbSearchContentOrderDate.SelectedItem as ComboBoxItem).Content.ToString().Replace(" ", ""));

                    switch (value)
                    {
                        case ValueDateSearch.All:
                            FromDate = null;
                            ToDate = null;
                            break;
                        case ValueDateSearch.Today:
                            FromDate = Today;
                            ToDate = Today;
                            break;
                        case ValueDateSearch.ThisWeek:
                            FromDate = this.GetStartOfCurrentWeek(Today);
                            ToDate = this.GetEndOfCurrentWeek(Today);
                            break;
                        case ValueDateSearch.ThisMonth:
                            FromDate = this.GetStartOfMonth(Today.Month, Today.Year);
                            ToDate = this.GetEndOfMonth(Today.Month, Today.Year);
                            break;
                        case ValueDateSearch.ThisQuarter:
                            FromDate = this.GetStartOfQuarter(Today.Year, Quarter(Today));
                            ToDate = this.GetEndOfQuarter(Today.Year, Quarter(Today));
                            break;
                        case ValueDateSearch.ThisYear:
                            FromDate = this.GetStartOfCurrentYear(Today);
                            ToDate = this.GetEndOfCurrentYear(Today);
                            break;
                        case ValueDateSearch.Yesterday:
                            FromDate = Today.AddDays(-1);
                            ToDate = Today.AddDays(-1);
                            break;
                        case ValueDateSearch.LastWeek:
                            FromDate = this.GetStartOfLastWeek(Today);
                            ToDate = this.GetEndOfLastWeek(Today);
                            break;
                        case ValueDateSearch.LastMonth:
                            FromDate = this.GetStartOfLastMonth(Today);
                            ToDate = this.GetEndOfLastMonth(Today);
                            break;
                        case ValueDateSearch.LastQuarter:
                            FromDate = this.GetStartOfLastQuarter(Today);
                            ToDate = this.GetEndOfLastQuarter(Today);
                            break;
                        case ValueDateSearch.LastYear:
                            FromDate = this.GetStartOfLastYear(Today);
                            ToDate = this.GetEndOfLastYear(Today);
                            break;
                        case ValueDateSearch.Last7Days:
                            FromDate = this.GetStartOfLastNumberDays(Today, 7);
                            ToDate = this.GetEndOfLastCurrentDay(Today);
                            break;
                        case ValueDateSearch.Last30Days:
                            FromDate = this.GetStartOfLastNumberDays(Today, 30);
                            ToDate = this.GetEndOfLastCurrentDay(Today);
                            break;
                        case ValueDateSearch.Last90Days:
                            FromDate = this.GetStartOfLastNumberDays(Today, 90);
                            ToDate = this.GetEndOfLastCurrentDay(Today);
                            break;
                        case ValueDateSearch.Last365Days:
                            FromDate = this.GetStartOfLastNumberDays(Today, 365);
                            ToDate = this.GetEndOfLastCurrentDay(Today);
                            break;

                        case ValueDateSearch.Tomorrow:
                            FromDate = Today.AddDays(1);
                            ToDate = Today.AddDays(1);
                            break;
                        case ValueDateSearch.NextWeek:
                            FromDate = this.GetStartOfNextWeek(Today);
                            ToDate = this.GetEndOfNextWeek(Today);
                            break;
                        case ValueDateSearch.NextMonth:
                            FromDate = this.GetStartOfNextMonth(Today);
                            ToDate = this.GetEndOfNextMonth(Today);
                            break;
                        case ValueDateSearch.NextQuarter:
                            FromDate = this.GetStartOfNextQuarter(Today);
                            ToDate = this.GetEndOfNextQuarter(Today);
                            break;
                        case ValueDateSearch.NextYear:
                            FromDate = this.GetStartOfNextYear(Today);
                            ToDate = this.GetEndOfNextYear(Today);
                            break;
                    }
                    cmbSearchContentOrderDate.Text = text;
                }

                if (null != Command)
                {
                    // Excute command search on viewmodel.
                    Command.Execute(base.GetValue(CommandParameterProperty));
                }
            }
        }

        #endregion

        #region Methods

        #region GetDaysInMonth

        /// <summary>
        ///get day number for current date 
        /// </summary>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private int GetDaysInMonth(int month, int year)
        {
            if (month < 1 || month > 12)
            {
                throw new System.ArgumentOutOfRangeException("month", month, "month must be between 1 and 12");
            }
            if (1 == month || 3 == month || 5 == month || 7 == month || 8 == month ||
            10 == month || 12 == month)
            {
                return 31;
            }
            else if (2 == month)
            {
                // Check for leap year
                if (0 == (year % 4))
                {
                    // If date is divisible by 400, it's a leap year.
                    // Otherwise, if it's divisible by 100 it's not.
                    if (0 == (year % 400))
                    {
                        return 29;
                    }
                    else if (0 == (year % 100))
                    {
                        return 28;
                    }

                    // Divisible by 4 but not by 100 or 400
                    // so it leaps
                    return 29;
                }
                // Not a leap year
                return 28;
            }
            return 30;
        }

        #endregion

        #region GetWeekNumber

        /// <summary>
        /// get week number for current date
        /// </summary>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        private int GetWeekNumber(DateTime fromDate)
        {
            // Get jan 1st of the year
            DateTime startOfYear = fromDate.AddDays(-fromDate.Day + 1).AddMonths(-fromDate.Month + 1);
            // Get dec 31st of the year
            DateTime endOfYear = startOfYear.AddYears(1).AddDays(-1);
            // ISO 8601 weeks start with Monday
            // The first week of a year includes the first Thursday, i.e. at least 4 days
            // DayOfWeek returns 0 for sunday up to 6 for Saturday
            int[] iso8601Correction = { 6, 7, 8, 9, 10, 4, 5 };
            int nds = fromDate.Subtract(startOfYear).Days + iso8601Correction[(int)startOfYear.DayOfWeek];
            int week = nds / 7;
            switch (week)
            {
                case 0:
                    // Return weeknumber of dec 31st of the previous year
                    return GetWeekNumber(startOfYear.AddDays(-1));
                case 53:
                    // If dec 31st falls before thursday it is week 01 of next year
                    if (endOfYear.DayOfWeek < DayOfWeek.Thursday)
                        return 1;
                    else
                        return week;
                default: return week;
            }
        }

        #endregion

        #region LastDays

        /// <summary>
        /// Get Date Start Of Last number Days
        /// </summary>
        /// <param name="date"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private DateTime GetStartOfLastNumberDays(DateTime date, int number)
        {
            DateTime dt = date.AddDays(-number);
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0); ;
        }
        /// <summary>
        /// Get Date End Of Last Current Day
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime GetEndOfLastCurrentDay(DateTime date)
        {
            DateTime dt = date.AddDays(-1);
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0); ;
        }

        #endregion

        #region Weeks

        /// <summary>
        /// Get Date Start Of Current Week
        /// </summary>
        /// <returns></returns>
        private DateTime GetStartOfCurrentWeek(DateTime date)
        {
            int daysToSubtract = (int)date.DayOfWeek;
            DateTime dt = date.Subtract(System.TimeSpan.FromDays(daysToSubtract));
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }
        /// <summary>
        /// Get Date End Of Current Week
        /// </summary>
        /// <returns></returns>
        private DateTime GetEndOfCurrentWeek(DateTime date)
        {
            DateTime dt = GetStartOfCurrentWeek(date).AddDays(6);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        /// <summary>
        /// Get date Start Of Last Week
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime GetStartOfLastWeek(DateTime date)
        {
            int DaysToSubtract = (int)date.DayOfWeek + 7;
            DateTime dt = date.Subtract(System.TimeSpan.FromDays(DaysToSubtract));
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }
        /// <summary>
        /// Get date end Of Last Week
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime GetEndOfLastWeek(DateTime date)
        {
            DateTime dt = GetStartOfLastWeek(date).AddDays(6);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        /// <summary>
        /// Get date Start Of Next Week
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime GetStartOfNextWeek(DateTime date)
        {
            int DaysToSubtract = 7 - (int)date.DayOfWeek;
            DateTime dt = date.Add(System.TimeSpan.FromDays(DaysToSubtract));
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }
        /// <summary>
        /// Get date end Of Next Week
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime GetEndOfNextWeek(DateTime date)
        {
            DateTime dt = GetStartOfNextWeek(date).AddDays(6);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        #endregion

        #region Months

        /// <summary>
        /// Get date Start of Current Month
        /// </summary>
        /// <param name="Month"></param>
        /// <param name="Year"></param>
        /// <returns></returns>
        private DateTime GetStartOfMonth(int month, int year)
        {
            return new DateTime(year, month, 1, 0, 0, 0, 0);
        }
        /// <summary>
        /// Get date End of Current Month
        /// </summary>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private DateTime GetEndOfMonth(int month, int year)
        {
            return new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, 999);
        }

        /// <summary>
        /// Get date Start Of Last Month
        /// </summary>
        /// <returns></returns>
        private DateTime GetStartOfLastMonth(DateTime date)
        {
            if (date.Month == 1)
                return GetStartOfMonth(12, Today.Year - 1);
            else
                return GetStartOfMonth(Today.Month - 1, Today.Year);
        }
        /// <summary>
        /// Get date Start Of Last Month
        /// </summary>
        /// <returns></returns>
        private DateTime GetEndOfLastMonth(DateTime date)
        {
            if (date.Month == 1)
                return GetEndOfMonth(12, Today.Year - 1);
            else
                return GetEndOfMonth(Today.Month - 1, Today.Year);
        }
        /// <summary>
        /// Get date Start Of Next Month
        /// </summary>
        /// <returns></returns>
        private DateTime GetStartOfNextMonth(DateTime date)
        {
            if (date.Month == 12)
                return GetStartOfMonth(1, Today.Year + 1);
            else
                return GetStartOfMonth(Today.Month + 1, Today.Year);
        }
        /// <summary>
        /// Get date Start Of Next Month
        /// </summary>
        /// <returns></returns>
        private DateTime GetEndOfNextMonth(DateTime date)
        {
            if (date.Month == 1)
                return GetEndOfMonth(12, Today.Year - 1);
            else
                return GetEndOfMonth(Today.Month + 1, Today.Year);
        }

        #endregion

        #region Quarter
        /// <summary>
        /// get Quarter of year
        /// </summary>
        /// <param name="dDate"></param>
        /// <returns></returns>
        private int Quarter(DateTime dDate)
        {
            //Get the current month
            int i = dDate.Month;

            //Based on the current month return the quarter
            if (i <= 3)
            { return 1; }
            else if (i >= 4 && i <= 6)
            { return 2; }
            else if (i >= 7 && i <= 9)
            { return 3; }
            else if (i >= 10 && i <= 12)
            { return 4; }
            else
                //Something probably is wrong 
                return 0;
        }
        /// <summary>
        /// get day start of quarter
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Qtr"></param>
        /// <returns></returns>
        private DateTime GetStartOfQuarter(int year, int quarter)
        {
            if (quarter == 1)    // 1st Quarter = January 1 to March 31
                return new DateTime(year, 1, 1, 0, 0, 0, 0);
            else if (quarter == 2) // 2nd Quarter = April 1 to June 30
                return new DateTime(year, 4, 1, 0, 0, 0, 0);
            else if (quarter == 3) // 3rd Quarter = July 1 to September 30
                return new DateTime(year, 7, 1, 0, 0, 0, 0);
            else // 4th Quarter = October 1 to December 31
                return new DateTime(year, 10, 1, 0, 0, 0, 0);
        }
        /// <summary>
        /// get day end of quarter
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Qtr"></param>
        /// <returns></returns>
        private DateTime GetEndOfQuarter(int year, int quarter)
        {
            if (quarter == 1)    // 1st Quarter = January 1 to March 31
                return new DateTime(year, 3, DateTime.DaysInMonth(year, 3), 23, 59, 59, 999);
            else if (quarter == 2) // 2nd Quarter = April 1 to June 30
                return new DateTime(year, 6, DateTime.DaysInMonth(year, 6), 23, 59, 59, 999);
            else if (quarter == 3) // 3rd Quarter = July 1 to September 30
                return new DateTime(year, 9, DateTime.DaysInMonth(year, 9), 23, 59, 59, 999);
            else // 4th Quarter = October 1 to December 31
                return new DateTime(year, 12, DateTime.DaysInMonth(year, 12), 23, 59, 59, 999);
        }
        /// <summary>
        /// get day Start of last quarter
        /// </summary>
        /// <returns></returns>
        private DateTime GetStartOfLastQuarter(DateTime date)
        {
            if (date.Month <= 3)
                //go to last quarter of previous year
                return GetStartOfQuarter(date.Year - 1, 4);
            else //return last quarter of current year
                return GetStartOfQuarter(date.Year, Quarter(date) - 1);
        }
        /// <summary>
        /// get day End of last quarter
        /// </summary>
        /// <returns></returns>
        private DateTime GetEndOfLastQuarter(DateTime date)
        {
            if (date.Month <= 3)
                //go to last quarter of previous year
                return GetEndOfQuarter(date.Year - 1, 4);
            else //return last quarter of current year
                return GetEndOfQuarter(date.Year, this.Quarter(date) - 1);
        }
        /// <summary>
        /// get day Start of Next quarter
        /// </summary>
        /// <returns></returns>
        private DateTime GetStartOfNextQuarter(DateTime date)
        {
            if (date.Month <= 3)
                //go to last quarter of previous year
                return GetStartOfQuarter(date.Year - 1, 4);
            else if (date.Month >= 10)
                //go to last quarter of previous year
                return GetStartOfQuarter(date.Year + 1, 1);
            else //return last quarter of current year
                return GetStartOfQuarter(date.Year, Quarter(date) + 1);
        }
        /// <summary>
        /// get day End of Next quarter
        /// </summary>
        /// <returns></returns>
        private DateTime GetEndOfNextQuarter(DateTime date)
        {
            if (date.Month <= 3)
                //go to last quarter of previous year
                return GetEndOfQuarter(date.Year - 1, 4);
            else if (date.Month >= 10)
                //go to last quarter of previous year
                return GetEndOfQuarter(date.Year + 1, 1);
            else //return last quarter of current year
                return GetEndOfQuarter(date.Year, this.Quarter(date) + 1);
        }

        #endregion

        #region Years

        /// <summary>
        /// get Date start of year
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        private DateTime GetStartOfYear(int Year)
        {
            return new DateTime(Year, 1, 1, 0, 0, 0, 0);
        }
        /// <summary>
        /// get Date end of year
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        private DateTime GetEndOfYear(int Year)
        {
            return new DateTime(Year, 12, DateTime.DaysInMonth(Year, 12), 23, 59, 59, 999);
        }
        /// <summary>
        /// get Date start of last year
        /// </summary>
        /// <returns></returns>
        private DateTime GetStartOfLastYear(DateTime date)
        {
            return GetStartOfYear(date.Year - 1);
        }
        /// <summary>
        /// get Date start of last year
        /// </summary>
        /// <returns></returns>
        private DateTime GetEndOfLastYear(DateTime date)
        {
            return GetEndOfYear(date.Year - 1);
        }
        /// <summary>
        /// get Date start of Next year
        /// </summary>
        /// <returns></returns>
        private DateTime GetStartOfNextYear(DateTime date)
        {
            return GetStartOfYear(date.Year + 1);
        }
        /// <summary>
        /// get Date start of Next Year
        /// </summary>
        /// <returns></returns>
        private DateTime GetEndOfNextYear(DateTime date)
        {
            return GetEndOfYear(date.Year + 1);
        }
        /// <summary>
        /// get Date start of year
        /// </summary>
        /// <returns></returns>
        private DateTime GetStartOfCurrentYear(DateTime date)
        {
            return GetStartOfYear(date.Year);
        }
        /// <summary>
        /// get Date start of year
        /// </summary>
        /// <returns></returns>
        private DateTime GetEndOfCurrentYear(DateTime date)
        {
            return GetEndOfYear(date.Year);
        }

        #endregion

        #endregion

        #region Properties

        #region ShowAllDate

        private bool _showAllDate = true;

        public bool ShowAllDate
        {
            get { return _showAllDate; }
            set { _showAllDate = value; }
        }

        #endregion

        private bool _showThisDate = true;

        public bool ShowThisDate
        {
            get { return _showThisDate; }
            set { _showThisDate = value; }
        }

        private bool _showLastDate = true;

        public bool ShowLastDate
        {
            get { return _showLastDate; }
            set { _showLastDate = value; }
        }

        private bool _showNextDate = true;

        public bool ShowNextDate
        {
            get { return _showNextDate; }
            set { _showNextDate = value; }
        }

        //private DateTime? _selectedFromDate = null;
        //public DateTime? SelectedFromDate
        //{
        //    get { return _selectedFromDate; }
        //    set
        //    {
        //        _selectedFromDate = value;
        //        OnPropertyChanged("SelectedFromDate");
        //        FromDate = _selectedFromDate;
        //        if (_selectedFromDate != null && SelectedToDate != null)
        //            this.cmbSearchContentOrderDate.Text = _selectedFromDate.Value.ToShortDateString() + " - " + SelectedToDate.Value.ToShortDateString();
        //        else
        //            this.cmbSearchContentOrderDate.Text = string.Empty;
        //    }
        //}

        //private DateTime? _selectedToDate = null;
        //public DateTime? SelectedToDate
        //{
        //    get { return _selectedToDate; }
        //    set
        //    {
        //        _selectedToDate = value;
        //        OnPropertyChanged("SelectedToDate");
        //        ToDate = _selectedToDate;
        //        if (SelectedFromDate != null && _selectedToDate != null)
        //            this.cmbSearchContentOrderDate.Text = SelectedFromDate.Value.ToShortDateString() + " - " + _selectedToDate.Value.ToShortDateString();
        //        else
        //            this.cmbSearchContentOrderDate.Text = string.Empty;
        //    }
        //}

        #endregion

        #region DependencyProperties

        #region Today

        [Category("Common Properties")]
        public DateTime Today
        {
            get { return (DateTime)GetValue(TodayProperty); }
            set { SetValue(TodayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Today.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TodayProperty =
            DependencyProperty.Register("Today", typeof(DateTime), typeof(DateComboBox), new PropertyMetadata(new PropertyChangedCallback(DateComboBox.TodayCallback)));

        /// <summary>
        /// Today Call Back of Today DependencyProperty
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public static void TodayCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            // Coerce the object to a DateComboBox
            DateComboBox comboboxDate = o as DateComboBox;
            if (e.NewValue != null)
            {
                comboboxDate.Today = (DateTime)e.NewValue;
            }

            if (null != comboboxDate.Command)
            {
                // Excute command search on viewmodel.
                comboboxDate.Command.Execute(comboboxDate.GetValue(CommandParameterProperty));
            }
        }

        #endregion

        #region FromDate

        [Category("Common Properties")]
        public DateTime? FromDate
        {
            get { return (DateTime?)GetValue(FromDateProperty); }
            set { SetValue(FromDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FromDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FromDateProperty =
            DependencyProperty.Register("FromDate", typeof(DateTime?), typeof(DateComboBox), new PropertyMetadata(new PropertyChangedCallback(DateComboBox.FromDateCallback)));
        /// <summary>
        /// FromDate Call Back of Today DependencyProperty
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public static void FromDateCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            // Coerce the object to a DateComboBox
            DateComboBox comboboxDate = o as DateComboBox;
            if (e.NewValue != null)
            {
                comboboxDate.FromDate = (DateTime?)e.NewValue;
                if (comboboxDate.ToDate != null)
                {
                    comboboxDate.cmbSearchContentOrderDate.Text = comboboxDate.FromDate.Value.ToShortDateString() + " - " + comboboxDate.ToDate.Value.ToShortDateString();
                }
                else
                    comboboxDate.cmbSearchContentOrderDate.Text = comboboxDate.FromDate.Value.ToShortDateString();
            }
            else if (comboboxDate.ToDate != null)
                comboboxDate.cmbSearchContentOrderDate.Text = comboboxDate.ToDate.Value.ToShortDateString();
            else
                comboboxDate.cmbSearchContentOrderDate.Text = string.Empty;

            if (null != comboboxDate.Command)
            {
                // Excute command search on viewmodel.
                comboboxDate.Command.Execute(comboboxDate.GetValue(CommandParameterProperty));
            }
        }

        #endregion

        #region ToDate

        public DateTime? ToDate
        {
            get { return (DateTime?)GetValue(ToDateProperty); }
            set { SetValue(ToDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Todate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToDateProperty =
            DependencyProperty.Register("ToDate", typeof(DateTime?), typeof(DateComboBox), new PropertyMetadata(new PropertyChangedCallback(DateComboBox.ToDateCallback)));
        /// <summary>
        /// ToDate Call Back of Today DependencyProperty
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public static void ToDateCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            // Coerce the object to a DateComboBox
            DateComboBox comboboxDate = o as DateComboBox;
            if (e.NewValue != null)
            {
                comboboxDate.ToDate = (DateTime?)e.NewValue;
                if (comboboxDate.FromDate != null)
                {
                    comboboxDate.cmbSearchContentOrderDate.Text = comboboxDate.FromDate.Value.ToShortDateString() + " - " + comboboxDate.ToDate.Value.ToShortDateString();
                }
                else
                    comboboxDate.cmbSearchContentOrderDate.Text = comboboxDate.ToDate.Value.ToShortDateString();
            }
            else if (comboboxDate.FromDate != null)
                comboboxDate.cmbSearchContentOrderDate.Text = comboboxDate.FromDate.Value.ToShortDateString();
            else
                comboboxDate.cmbSearchContentOrderDate.Text = string.Empty;

            if (null != comboboxDate.Command)
            {
                // Excute command search on viewmodel.
                comboboxDate.Command.Execute(comboboxDate.GetValue(CommandParameterProperty));
            }
        }

        #endregion

        #endregion

        #region INotifyPropertyChanged handling

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    public enum ValueDateSearch
    {
        All = 0,
        Today = 1,
        ThisWeek = 2,
        ThisMonth = 3,
        ThisQuarter = 4,
        ThisYear = 5,
        Yesterday = 6,
        LastWeek = 7,
        LastMonth = 8,
        LastQuarter = 9,
        LastYear = 10,
        Last7Days = 11,
        Last30Days = 12,
        Last90Days = 13,
        Last365Days = 14,
        Tomorrow = 15,
        NextWeek = 16,
        NextMonth = 17,
        NextQuarter = 18,
        NextYear = 19
    }
}
