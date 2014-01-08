using System.Windows;
using System.Windows.Input;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupUpdateOrderCostViewModel : ViewModelBase
    {
        #region Defines

        #endregion

        #region Properties

        private int _updateOrderCostOption;
        /// <summary>
        /// Gets or sets the UpdateOrderCostOption.
        /// </summary>
        public int UpdateOrderCostOption
        {
            get { return _updateOrderCostOption; }
            set
            {
                if (_updateOrderCostOption != value)
                {
                    this.IsDirty = true;
                    _updateOrderCostOption = value;
                    OnPropertyChanged(() => UpdateOrderCostOption);
                }
            }
        }

        private decimal _newOrderCost;
        /// <summary>
        /// Gets or sets the NewOrderCost.
        /// </summary>
        public decimal NewOrderCost
        {
            get { return _newOrderCost; }
            set
            {
                if (_newOrderCost != value)
                {
                    this.IsDirty = true;
                    _newOrderCost = value;
                    OnPropertyChanged(() => NewOrderCost);
                }
            }
        }

        private decimal _currentOrderCost;
        /// <summary>
        /// Gets or sets the CurrentOrderCost.
        /// </summary>
        public decimal CurrentOrderCost
        {
            get { return _currentOrderCost; }
            set
            {
                if (_currentOrderCost != value)
                {
                    _currentOrderCost = value;
                    OnPropertyChanged(() => CurrentOrderCost);
                }
            }
        }

        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupUpdateOrderCostViewModel(decimal currentOrderCost)
        {
            CurrentOrderCost = currentOrderCost;

            InitialCommand();
        }

        #endregion

        #region Command Methods

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return IsDirty;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        #endregion
    }
}