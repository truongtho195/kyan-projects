using System.Windows;
using System.Windows.Input;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PromotionReasonViewModel : ViewModelBase
    {
        #region Properties

        private string _reasonReactive;
        /// <summary>
        /// Gets or sets the ReasonReactive.
        /// </summary>
        public string ReasonReactive
        {
            get { return _reasonReactive; }
            set
            {
                if (_reasonReactive != value)
                {
                    IsValid = true;
                    _reasonReactive = value;
                    OnPropertyChanged(() => ReasonReactive);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PromotionReasonViewModel()
        {
            InitialCommand();
        }

        public PromotionReasonViewModel(string reasonReactive)
            : this()
        {
            // Set reason reactive value
            ReasonReactive = reasonReactive;
            IsValid = false;
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
            return IsValid && !string.IsNullOrWhiteSpace(ReasonReactive);
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