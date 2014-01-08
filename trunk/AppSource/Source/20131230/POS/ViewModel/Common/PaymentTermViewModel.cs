using System;
using System.Windows.Input;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class PaymentTermViewModel : ViewModelBase
    {
        #region Fields

        private bool _isDirty = false;
        private string _descriptionFormat1 = "{0}% {1} Net {2}";
        private string _descriptionFormat2 = "Net {0}";
        private string _descriptionFormat3 = "Due on receipt";

        #endregion

        #region Contructors

        public PaymentTermViewModel(short dueDays, decimal discount, short discountDays)
        {
            _ownerViewModel = this;

            _dueDays = dueDays;
            _discount = discount;
            _discountDays = discountDays;
        }

        #endregion

        #region Properties

        #region DueDays

        private short _dueDays;
        /// <summary>
        /// Gets or sets DueDay.
        /// </summary>
        public short DueDays
        {
            get
            {
                return _dueDays;
            }
            set
            {
                if (_dueDays != value)
                {
                    _isDirty = true;
                    _dueDays = value;
                    OnPropertyChanged(() => DueDays);
                }
            }
        }

        #endregion

        #region Discount

        private decimal _discount;
        /// <summary>
        /// Gets or sets discount.
        /// </summary>
        public decimal Discount
        {
            get
            {
                return _discount;
            }
            set
            {
                if (_discount != value)
                {
                    _isDirty = true;
                    _discount = value;
                    OnPropertyChanged(() => Discount);
                }
            }
        }

        #endregion

        #region DiscountDays

        private short _discountDays;
        /// <summary>
        /// Gets or sets DueDay.
        /// </summary>
        public short DiscountDays
        {
            get
            {
                return _discountDays;
            }
            set
            {
                if (_discountDays != value)
                {
                    _isDirty = true;
                    _discountDays = value;
                    OnPropertyChanged(() => DiscountDays);
                }
            }
        }

        #endregion

        #region Description

        private string _description;
        /// <summary>
        /// Gets description.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
            private set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(() => Description);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// When 'Save' button clicked, command will executes.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(SaveExecute, CanSaveExecute);
                }
                return _saveCommand;
            }
        }

        #endregion

        #region CancelCommand

        private ICommand _cancelCommand;
        /// <summary>
        /// When 'Cancel' button clicked, command will executes.
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(CancelExecute);
                }
                return _cancelCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region SaveExecute

        /// <summary>
        /// Save.
        /// </summary>
        private void SaveExecute()
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        /// <summary>
        /// Determine SaveExecute method can execute or not.
        /// </summary>
        /// <returns>True is execute.</returns>
        private bool CanSaveExecute()
        {
            return _isDirty;
        }

        #endregion

        #region CancelExecute

        /// <summary>
        /// Cancel.
        /// </summary>
        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Private Methods

        #region Save

        /// <summary>
        /// Save department, category, brand.
        /// </summary>
        private void Save()
        {
            try
            {
                if (_discountDays >= _dueDays)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("The Number of day to pay away for discount must less than the number of due day.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
                else
                {
                    if (_dueDays > 0)
                    {
                        if (_discount > 0 && _discountDays > 0)
                        {
                            _description = string.Format(_descriptionFormat1, _discount, _discountDays, _dueDays);
                        }
                        else
                        {
                            _description = string.Format(_descriptionFormat2, _dueDays);
                        }
                    }
                    else
                    {
                        _description = _descriptionFormat3;
                    }

                    FindOwnerWindow(this).DialogResult = true;
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            _dueDays = 0;
            _discountDays = 0;
            _discount = 0;
            _description = null;

            FindOwnerWindow(this).DialogResult = false;
        }

        #endregion

        #endregion
    }
}