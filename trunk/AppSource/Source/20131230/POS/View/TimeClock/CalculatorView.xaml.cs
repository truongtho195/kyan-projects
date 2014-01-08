using System;
using System.Collections.Generic;
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
using System.Windows.Controls.Primitives;
using CPC.Toolkit.Base;
using System.Text.RegularExpressions;
using CPC.Service;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for CalculatorView.xaml
    /// </summary>
    public partial class CalculatorView
    {
        #region Fields

        private IDialogService _dialogService;
        private object _ownerViewModel;

        /// <summary>
        /// Determine number overflowed or not.
        /// </summary>
        private bool _isNumberOverflowed = false;
        /// <summary>
        /// Length of time input.
        /// </summary>
        private readonly short _timeLength = 6;
        /// <summary>
        /// Example 14 or 14: or 14. or .14 or :14 or 14:50 or 14.50
        /// </summary>
        private readonly string _timePattern = @"^\d+$|^\d+[.:]$|^[.:]\d+$|^\d+[.:]\d+$";
        /// <summary>
        /// Example 14 or 14:
        /// </summary>
        private readonly string _hourPattern1 = @"^\d+$|^\d+[:]$";
        /// <summary>
        /// Example :14
        /// </summary>
        private readonly string _hourPattern2 = @"^[:]\d+$";
        /// <summary>
        /// Example 14:50
        /// </summary>
        private readonly string _hourPattern3 = @"^\d+[:]\d+$";
        /// <summary>
        /// Save operator input, default is "+".
        /// </summary>
        private string _currentOperator = "+";
        /// <summary>
        /// Operand list used for show operands, operators and result.
        /// </summary>
        private List<string> _operandList = new List<string>();
        /// <summary>
        /// Operand queue used for calculate result.
        /// </summary>
        private Queue<TimeSpan> _operandQueue = new Queue<TimeSpan>();

        #endregion

        #region Contructors

        public CalculatorView()
        {
            this.InitializeComponent();

            _dialogService = ServiceLocator.Resolve<IDialogService>();
            _ownerViewModel = App.Current.MainWindow.DataContext;

            // When buttons in gridNumBoard clicked,  click event of buttons will bubble to click event gridNumBoard.
            gridNumBoard.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ButtonClick));

            KeyBinding keyBinding = new KeyBinding(ApplicationCommands.NotACommand, Key.V, ModifierKeys.Control);
            textBoxInputTime.ContextMenu = null;
            textBoxInputTime.AllowDrop = false;
            textBoxInputTime.MaxLength = _timeLength;
            textBoxInputTime.InputBindings.Add(keyBinding);
            keyBinding = new KeyBinding(ApplicationCommands.NotACommand, Key.Insert, ModifierKeys.Shift);
            textBoxInputTime.InputBindings.Add(keyBinding);
            textBoxInputTime.PreviewKeyDown += TextBoxPreviewKeyDown;
            textBoxInputTime.PreviewTextInput += TextBoxPreviewTextInput;

            timePickerStartHours.RealTime = DateTime.Today;
            timePickerStartHours.ValueChanged += TimePickerStartHoursValueChanged;
            timePickerEndHours.ValueChanged += TimePickerEndHoursValueChanged;
            timePickerEndHours.RealTime = DateTime.Today;

            timePickerInputTime.RealTime = DateTime.Today;
            timePickerInputTime.ValueChanged += TimePickerInputTimeValueChanged;
            //maskedTextBoxInputTimeDecimal.ValueChanged += MaskedTextBoxInputTimeDecimalValueChanged;
            

            buttonClose.Click += ButtonCloseClick;
        }
       

    

        #endregion

        #region Methods

        /// <summary>
        /// Button "CE" click.
        /// </summary>
        private void ButtonClearEntryClick()
        {
            ClearEntry();
        }

        /// <summary>
        /// Button "C" click.
        /// </summary>
        private void ButtonClearClick()
        {
            Clear();
        }

        /// <summary>
        /// Button "-" click.
        /// </summary>
        private void ButtonSubtractionClick()
        {
            AddOperand(textBoxInputTime.Text);
            _currentOperator = "-";
            textBoxInputTime.Focus();
        }

        /// <summary>
        /// Button "+" click.
        /// </summary>
        private void ButtonAdditionClick()
        {
            AddOperand(textBoxInputTime.Text);
            _currentOperator = "+";
            textBoxInputTime.Focus();
        }

        /// <summary>
        /// Button "=" click.
        /// </summary>
        private void ButtonEqualClick()
        {
            Reset();
            AddOperand(textBoxInputTime.Text);
            _currentOperator = "+";

            if (_operandQueue.Count > 1)
            {
                try
                {
                    TimeSpan total = CalculateTotalTime();
                    _operandList.Add("_____________________");
                    _operandList.Add(string.Format("[{0}]    {1}", Math.Round(total.Truncate(TimeSpan.TicksPerMinute).TotalHours, 2), total.ToHours()));

                    // Refresh result.
                    textBoxResult.Text = string.Join(Environment.NewLine, _operandList);
                    textBoxResult.ScrollToEnd();
                }
                catch (OverflowException ex)
                {
                    _dialogService.ShowMessageBox(_ownerViewModel, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Button ":" click.
        /// </summary>
        private void ButtonColonsClick()
        {
            bool isContainsDot = textBoxInputTime.Text.Contains(".");
            bool isContainsColons = textBoxInputTime.Text.Contains(":");
            if (!isContainsColons && !isContainsDot)
            {
                InsertCharacter(":");
            }
            else
            {
                textBoxInputTime.Focus();
            }
        }

        /// <summary>
        /// Button "." click.
        /// </summary>
        private void ButtonDotClick()
        {
            bool isContainsDot = textBoxInputTime.Text.Contains(".");
            bool isContainsColons = textBoxInputTime.Text.Contains(":");
            if (!isContainsColons && !isContainsDot)
            {
                InsertCharacter(".");
            }
            else
            {
                textBoxInputTime.Focus();
            }
        }

        /// <summary>
        /// Clear current value input.
        /// </summary>
        private void ClearEntry()
        {
            textBoxInputTime.Clear();
            textBoxInputTime.Focus();
        }

        /// <summary>
        /// Clear all value input.
        /// </summary>
        private void Clear()
        {
            ClearEntry();
            textBoxResult.Clear();
            _operandList.Clear();
            _operandQueue.Clear();
            _currentOperator = "+";
        }

        /// <summary>
        /// Reset when number overflowed.
        /// </summary>
        private void Reset()
        {
            if (_isNumberOverflowed)
            {
                textBoxResult.Clear();
                _operandList.Clear();
                _operandQueue.Clear();
                _isNumberOverflowed = false;
            }
        }

        /// <summary>
        /// Calculate sum of all value input.
        /// </summary>
        /// <returns></returns>
        private TimeSpan CalculateTotalTime()
        {
            try
            {
                TimeSpan total = TimeSpan.Zero;
                while (_operandQueue.Count > 0)
                {
                    total += _operandQueue.Dequeue();
                }

                // Add result to operand queue for next calculate.
                _operandQueue.Enqueue(total);
                return total;
            }
            catch (OverflowException)
            {
                _isNumberOverflowed = true;
                throw;
            }
        }

        private void AddOperand(string operand)
        {
            Regex timePattern = new Regex(_timePattern);
            // Add operand when operand match time pattern.
            if (timePattern.IsMatch(operand))
            {
                ClearEntry();

                TimeSpan duration = TimeSpan.Zero;
                bool isDecimal = operand.Contains(".");
                if (isDecimal)
                {
                    // Input decimal.
                    duration = TimeSpan.FromHours(Double.Parse(operand));
                }
                else
                {
                    // Input time.
                    if ((new Regex(_hourPattern1)).IsMatch(operand))
                    {
                        duration = new TimeSpan(int.Parse(operand.Split(':')[0]), 0, 0);
                    }
                    else if ((new Regex(_hourPattern2)).IsMatch(operand))
                    {
                        duration = new TimeSpan(0, int.Parse(operand.Split(':')[1]), 0);
                    }
                    else if ((new Regex(_hourPattern3)).IsMatch(operand))
                    {
                        duration = new TimeSpan(int.Parse(operand.Split(':')[0]), int.Parse(operand.Split(':')[1]), 0);
                    }
                }

                _operandList.Add(string.Format("{0} {1}", _currentOperator, duration.ToHours()));
                if (_currentOperator == "-")
                {
                    duration = -duration;
                }
                _operandQueue.Enqueue(duration);
                textBoxResult.Text = string.Join(Environment.NewLine, _operandList);
                textBoxResult.ScrollToEnd();
            }
            else
            {
                textBoxInputTime.Focus();
            }
        }

        /// <summary>
        /// Insert character into time.
        /// </summary>
        /// <param name="character">Character to insert.</param>
        private void InsertCharacter(string character)
        {
            Reset();
            textBoxInputTime.Focus();

            bool isSmallerThanTimeLength = textBoxInputTime.Text.Length < _timeLength;
            if (isSmallerThanTimeLength)
            {
                int selectionStart = textBoxInputTime.SelectionStart;
                string newText = textBoxInputTime.Text.Insert(selectionStart, character);
                textBoxInputTime.Text = newText;
                textBoxInputTime.SelectionStart = selectionStart + 1;
            }
        }

        private void SelectAction(string entry)
        {
            // Determine button clicked.
            switch (entry)
            {
                case "CE":
                    ButtonClearEntryClick();
                    break;
                case "C":
                    ButtonClearClick();
                    break;
                case "-":
                    ButtonSubtractionClick();
                    break;
                case "+":
                    ButtonAdditionClick();
                    break;
                case "=":
                    ButtonEqualClick();
                    break;
                case ":":
                    ButtonColonsClick();
                    break;
                case ".":
                    ButtonDotClick();
                    break;
                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    InsertCharacter(entry);
                    break;
            }
        }

        /// <summary>
        /// Calculate and show difference hours, minutes. Total minutes.
        /// </summary>
        /// <param name="startHours">Start hours</param>
        /// <param name="endHours">End hours</param>
        private void ShowInterval(DateTime startHours, DateTime endHours)
        {
            TimeSpan interval = endHours.TimeOfDay.Truncate(TimeSpan.TicksPerMinute) - startHours.TimeOfDay.Truncate(TimeSpan.TicksPerMinute);
            textBlockHours.Text = interval.Hours.ToString();
            textBlockMinutes.Text = interval.Minutes.ToString();
            textBlockTotalMinutes.Text = interval.TotalMinutes.ToString();
        }

        #endregion

        #region Events

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            SelectAction((e.Source as Button).Tag as string);
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                // Disallow the space key, which doesn't raise a PreviewTextInput event.
                e.Handled = true;
            }
        }

        private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow number key, colons key, and dot key.
            short value;
            bool isNumeric = Int16.TryParse(e.Text, out value);

            if (isNumeric)
            {
                Reset();
                e.Handled = false;
            }
            else
            {
                string entry = null;
                if (e.Text == "." || e.Text == ":")
                {
                    entry = checkBoxUseColons.IsChecked == true ? ":" : e.Text;
                }
                else
                {
                    entry = e.Text == "\r" ? "=" : e.Text;
                }
                SelectAction(entry);
                e.Handled = true;
            }
        }

        private void TimePickerStartHoursValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ShowInterval(timePickerStartHours.RealTime.Value, timePickerEndHours.RealTime.Value);
        }

        private void TimePickerEndHoursValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ShowInterval(timePickerStartHours.RealTime.Value, timePickerEndHours.RealTime.Value);
        }

        private void TimePickerInputTimeValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Change value in MaskedTextBoxInputTimeDecimal when TimePickerInputTime focused.
            if (timePickerInputTime.IsFocused)
            {
                double totalHours = Math.Round(timePickerInputTime.RealTime.Value.TimeOfDay.Truncate(TimeSpan.TicksPerMinute).TotalHours, 2);

                // Update correct format of MaskedTextBoxInputTimeDecimal.
                string valueCorrect = (int)totalHours < 10 ? string.Format("0{0}", totalHours.ToString()) : totalHours.ToString();

                maskedTextBoxInputTimeDecimal.Value = valueCorrect;
            }
        }

        private void MaskedTextBoxInputTimeDecimalValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Reset 00h when input time over 24h.
                string hours = e.NewValue.ToString().Split('.')[0];
                if (short.Parse(hours) >= 24)
                {
                    maskedTextBoxInputTimeDecimal.Value = maskedTextBoxInputTimeDecimal.Value.ToString().Replace(hours, "00");
                }

                // Change value in TimePickerInputTime when MaskedTextBoxInputTimeDecimal focused.
                if (maskedTextBoxInputTimeDecimal.IsFocused)
                {
                    TimeSpan duration = TimeSpan.FromHours(Double.Parse(maskedTextBoxInputTimeDecimal.Value.ToString()));
                    DateTime datetime = DateTime.Today.AddTicks(duration.Ticks);
                    timePickerInputTime.RealTime = datetime;
                }
            }));
        }

        private void ButtonCloseClick(object sender, RoutedEventArgs e)
        {
            DependencyObject depObj = VisualTreeHelper.GetParent(this);
            while (depObj as Window == null)
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }
            (depObj as Window).Close();
        }

        #endregion
    }
}