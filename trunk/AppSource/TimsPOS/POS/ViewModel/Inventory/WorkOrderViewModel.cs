using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using System.Windows.Input;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class WorkOrderViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Contructors

        public WorkOrderViewModel(bool isList)
        {
            IsSearchMode = isList;
        }

        #endregion

        #region Properties

        #region IsSearchMode

        private bool _isSearchMode;
        /// <summary>
        /// Open search component when IsSearchMode property is true.
        /// Close search component when IsSearchMode property is false.
        /// </summary>
        public bool IsSearchMode
        {
            get
            {
                return _isSearchMode;
            }
            set
            {
                if (_isSearchMode != value)
                {
                    _isSearchMode = value;
                    OnPropertyChanged(() => IsSearchMode);
                }
            }
        }

        #endregion

        #endregion

        #region Commands

        #region OpenSearchComponentCommand

        private ICommand _openSearchComponentCommand;
        public ICommand OpenSearchComponentCommand
        {
            get
            {
                if (_openSearchComponentCommand == null)
                {
                    _openSearchComponentCommand = new RelayCommand(OpenSearchComponentExecute);
                }
                return _openSearchComponentCommand;
            }
        }

        #endregion

        #region CloseSearchComponentCommand

        private ICommand _closeSearchComponentCommand;
        public ICommand CloseSearchComponentCommand
        {
            get
            {
                if (_closeSearchComponentCommand == null)
                {
                    _closeSearchComponentCommand = new RelayCommand(CloseSearchComponentExecute, CanCloseSearchComponentExecute);
                }
                return _closeSearchComponentCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region OpenSearchComponentExecute

        private void OpenSearchComponentExecute()
        {
            IsSearchMode = true;
        }

        #endregion

        #region CloseSearchComponentExecute

        private void CloseSearchComponentExecute()
        {
            IsSearchMode = false;
        }

        #endregion

        #region CanCloseSearchComponentExecute

        private bool CanCloseSearchComponentExecute()
        {
            return true;
        }

        #endregion

        #endregion

        #region Methods

        #endregion
    }
}
