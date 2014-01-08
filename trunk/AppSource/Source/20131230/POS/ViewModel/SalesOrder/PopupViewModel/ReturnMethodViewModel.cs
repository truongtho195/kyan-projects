using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using System.Windows;
using System.ComponentModel;

namespace CPC.POS.ViewModel
{
    public class ReturnMethodViewModel : ViewModelBase
    {
        #region Define

        #endregion

        #region Constructors
        public ReturnMethodViewModel()
        {
            InitialCommand();
        }
        #endregion

        #region Properties

        #region IsStoreCard
        private bool _isStoreCard = true;
        /// <summary>
        /// Gets or sets the IsStoreCard.
        /// </summary>
        public bool IsStoreCard
        {
            get { return _isStoreCard; }
            set
            {
                if (_isStoreCard != value)
                {
                    _isStoreCard = value;
                    OnPropertyChanged(() => IsStoreCard);
                }
            }
        }
        #endregion

        #endregion

        #region Commands Methods

        #region OkCommand
        /// <summary>
        /// Gets the Ok Command.
        /// <summary>

        public RelayCommand<object> OkCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Ok command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            Window window = FindOwnerWindow(_ownerViewModel);
            window.DialogResult = true;
        }
        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>

        public RelayCommand<object> CancelCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute(object param)
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        } 
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }
        #endregion

        #region Public Methods
        #endregion
    }


}
