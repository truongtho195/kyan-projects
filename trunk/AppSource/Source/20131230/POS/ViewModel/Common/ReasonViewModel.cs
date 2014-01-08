using System.Windows;
using System.Windows.Input;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class ReasonViewModel : ViewModelBase
    {
      
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public ReasonViewModel()
        {
            InitialCommand();
        }

        public ReasonViewModel(string reason)
            : this()
        {
            // Set reason reactive value
            Reason = reason;
            IsDirty = false;
        }

        #endregion

        #region Properties

        private string _reason;
        /// <summary>
        /// Gets or sets the ReasonReactive.
        /// </summary>
        public string Reason
        {
            get { return _reason; }
            set
            {
                if (_reason != value)
                {
                    _reason = value;
                    IsDirty = true;
                    OnPropertyChanged(() => Reason);
                }
            }
        }


        #region IsDirty
        private bool _isDirty=false;
        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(() => IsDirty);
                }
            }
        }
        #endregion


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
            return IsDirty && !string.IsNullOrWhiteSpace(Reason);
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