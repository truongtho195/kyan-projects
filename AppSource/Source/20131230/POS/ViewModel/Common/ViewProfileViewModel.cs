using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class ViewProfileViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand OKCommand { get; private set; }

        #endregion

        #region Constructors
        public ViewProfileViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
        }
        #endregion

        #region Properties

        #region GuestModel
        private base_GuestModel _guestModel;
        /// <summary>
        /// Gets or sets the GuestModel.
        /// </summary>
        public base_GuestModel GuestModel
        {
            get { return _guestModel; }
            set
            {
                if (_guestModel != value)
                {
                    _guestModel = value;
                    OnPropertyChanged(() => GuestModel);
                }
            }
        }
        #endregion
        #endregion

        #region Commands Methods

        #region OKCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOKCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOKCommandExecute()
        {
            System.Windows.Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            // Route the commands
            OKCommand = new RelayCommand(OnOKCommandExecute, OnOKCommandCanExecute);
        }
        #endregion

        #region Public Methods
        #endregion
    }
}