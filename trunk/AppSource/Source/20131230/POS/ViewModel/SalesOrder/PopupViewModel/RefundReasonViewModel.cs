using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using System.Windows;

namespace CPC.POS.ViewModel
{
    class RefundReasonViewModel : ViewModelBase
    {
        #region Define

        #endregion

        #region Constructors
        public RefundReasonViewModel()
        {
            _ownerViewModel = this;
            IsDirty =false;
            InitialCommand();
        }
        #endregion

        #region Properties


        #region IsDirty
        private bool _isDirty;
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

        #region Reason
        private string _reason;
        /// <summary>
        /// Gets or sets the Reason.
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
        #endregion
        #endregion

        #region Commands Methods

        #region OKCommand
        /// <summary>
        /// Gets the OK Command.
        /// <summary>

        public RelayCommand<object> OKCommand { get; private set; }



        /// <summary>
        /// Method to check whether the OK command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOKCommandCanExecute(object param)
        {
            return IsDirty && !string.IsNullOrWhiteSpace(Reason);
        }


        /// <summary>
        /// Method to invoke when the OK command is executed.
        /// </summary>
        private void OnOKCommandExecute(object param)
        {
          FindOwnerWindow(_ownerViewModel).DialogResult = true;
           
        } 
        #endregion

        #region Cancel

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
            OKCommand = new RelayCommand<object>(OnOKCommandExecute, OnOKCommandCanExecute);

            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }
        #endregion

    }


}
