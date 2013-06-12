using System.Windows;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using SecurityLib;

namespace CPC.POS.ViewModel
{
    class ChangePasswordViewModel : ViewModelBase
    {
        #region Defines

        private base_ResourceAccountRepository _resourceAccountRepository = new base_ResourceAccountRepository();

        #endregion

        #region Properties

        private base_ResourceAccountModel _resourceAccountModel;
        /// <summary>
        /// Gets or sets the ResourceAccountModel.
        /// </summary>
        public base_ResourceAccountModel ResourceAccountModel
        {
            get { return _resourceAccountModel; }
            set
            {
                if (_resourceAccountModel != value)
                {
                    _resourceAccountModel = value;
                    OnPropertyChanged(() => ResourceAccountModel);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public ChangePasswordViewModel()
        {
            InitialCommand();

            ResourceAccountModel = new base_ResourceAccountModel();
            ResourceAccountModel.LoginName = Define.USER.LoginName;
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
            return IsValid;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            if (ResourceAccountModel.NewPassword.Equals(ResourceAccountModel.NewConfirmPassword))
            {
                // Encrypt old password
                string encryptOldPassword = AESSecurity.Encrypt(ResourceAccountModel.OldPassword);

                // Get resource account
                base_ResourceAccount resourceAccount = _resourceAccountRepository.Get(x => x.LoginName.Equals(ResourceAccountModel.LoginName));

                // Check valid of old password
                if (resourceAccount != null && resourceAccount.Password.Equals(encryptOldPassword))
                {
                    // Encrypt new password
                    string encryptNewPassword = AESSecurity.Encrypt(ResourceAccountModel.NewPassword);

                    // Update new password
                    resourceAccount.Password = encryptNewPassword;

                    // Accept change
                    _resourceAccountRepository.Commit();

                    Window window = FindOwnerWindow(this);
                    window.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Old password is not match", "POS", MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show("Confirm password is not match", "POS", MessageBoxButton.OK);
            }
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
