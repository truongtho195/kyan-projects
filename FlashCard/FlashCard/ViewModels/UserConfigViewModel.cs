using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlashCard.Views;
using System.Waf.Applications;
using FlashCard.Model;
using System.Windows.Input;
using MVVMHelper.Commands;

namespace FlashCard.ViewModels
{
    public class UserConfigViewModel : ViewModel<UserConfigView>
    {
        #region Constructors
        public UserConfigViewModel(UserConfigView view)
            : base(view)
        {
            
        } 
        #endregion

        #region Properties

        #region SetupModel
        private SetupModel _setupModel;
        /// <summary>
        /// Gets or sets the SetupModel.
        /// </summary>
        public SetupModel SetupModel
        {
            get { return _setupModel; }
            set
            {
                if (_setupModel != value)
                {
                    _setupModel = value;
                    SelectedSetupModel = new SetupModel();
                    SelectedSetupModel = _setupModel;
                    RaisePropertyChanged(() => SetupModel);
                }
            }
        }
        #endregion


        #region SelectedSetupModel
        private SetupModel _selectedSetupModel;
        /// <summary>
        /// Gets or sets the SelectedSetupModel.
        /// </summary>
        public SetupModel SelectedSetupModel
        {
            get { return _selectedSetupModel; }
            set
            {
                if (_selectedSetupModel != value)
                {
                    _selectedSetupModel = value;
                    RaisePropertyChanged(() => SelectedSetupModel);
                }
            }
        }
        #endregion


        #endregion

        #region Commands

        #region OkCommand
        /// <summary>
        /// Gets the Ok Command.
        /// <summary>
        private ICommand _okCommand;
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                    _okCommand = new RelayCommand(this.OnOkExecute, this.OnOkCanExecute);
                return _okCommand;
            }
        }

        /// <summary>
        /// Method to check whether the Ok command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkExecute(object param)
        {
            this.ViewCore.DialogResult = true;
            SetupModel = SelectedSetupModel;
            this.ViewCore.Close();
                
        } 
        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>
        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand(this.OnCancelExecute, this.OnCancelCanExecute);
                return _cancelCommand;
            }
        }

        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelExecute(object param)
        {
            this.ViewCore.DialogResult = false;
            this.ViewCore.Close();
        }
        #endregion

        #endregion


    }
}
